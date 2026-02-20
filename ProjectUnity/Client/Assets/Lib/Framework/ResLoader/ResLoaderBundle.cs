using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �������ڣ� 2020/10/20 16:50:51
/// ����AssetBundle�е��ز�
/// ��AssetBundleManager�м��ض�Ӧ���ز�
/// </summary>
public class ResLoaderBundle : IResLoader
{
    /// <summary>
    /// bundle�ļ���׺
    /// </summary>
    public static string extension = "asset";
    /// <summary>
    /// �����б�
    /// </summary>
    private Dictionary<string, string[]> _dependList;

    /// <summary>
    /// �ȴ��첽���صĶ��У����㷴��
    /// </summary>
    public Dictionary<string, ABUnit> _loadAsyncWaitDic;
    /// <summary>
    /// ���ڼ��صĶ���
    /// </summary>
    public Dictionary<string, ABUnit> _loadingAsyncDic;
    /// <summary>
    /// �Ѿ����غõ��ز��б�
    /// </summary>
    public Dictionary<string, ABUnit> _loadedAssetDic;
    /// <summary>
    /// �ȴ���ɾ�����ز��б�
    /// �ڵȴ�ɾ����;��Ҫ����Ҫ���ؾ��ó�����
    /// </summary>
    public Dictionary<string, ABUnit> _unloadWaitList;
    /// <summary>
    /// ������ʱ�洢Ҫ��������Դ����
    /// </summary>
    private List<ABUnit> _tempABUnitList;

    /// <summary>
    /// �Ѿ����غõ��ز��б�
    /// </summary>
    public Dictionary<string, int> fileNames = new Dictionary<string, int>();


    public ResLoaderBundle()
    {
        Init();
    }
    /// <summary>
    /// ��ȡ�����б�
    /// </summary>
    private void GetDependList()
    {
        _dependList = new Dictionary<string, string[]>();
        //��ȡ����
        AssetBundle main;
        // AssetBundle.SetAssetBundleDecryptKey(HotfixSetting.Instance.bundleKey);
        if (GameSetting.Instance.IsResInPatch("Asset"))
        {
            main = AssetBundle.LoadFromFile(GameSetting.Instance.abPatchAddress + "Asset");
        }
        else
        {
            main = AssetBundle.LoadFromFile(GameSetting.Instance.abAddressLocal + "Asset");
        }

        //AssetBundle.SetAssetBundleDecryptKey(null);
        if (main != null)
        {
            AssetBundleManifest manifest = main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest != null)
            {
                string[] bundleList = manifest.GetAllAssetBundles();
                string[] depend;
                string bundleName;
                int length = bundleList.Length;
                for (int i = 0; i < length; i++)
                {
                    bundleName = bundleList[i];
                    depend = manifest.GetAllDependencies(bundleName);
                    if (depend.Length > 0)
                    {
                        _dependList[bundleName] = depend;
                    }
                }
            }
        }
        main.Unload(true);
    }
    private void Init()
    {
        _loadAsyncWaitDic = new Dictionary<string, ABUnit>();
        _loadingAsyncDic = new Dictionary<string, ABUnit>();
        _loadedAssetDic = new Dictionary<string, ABUnit>();
        _unloadWaitList = new Dictionary<string, ABUnit>();
        _tempABUnitList = new List<ABUnit>();
        fileNames = new Dictionary<string, int>();
        //GetDependList();
        OEF.Instance.AddCantPauseLink(this, Update);
    }

    public void InitParams()
    {
        if (_dependList == null || _dependList.Count == 0)
        {
            GetDependList();
        }
    }
    public void Clear()
    {
        UnloadRes(ABUnit.DisposType.ChangeScene);
        _loadAsyncWaitDic.Clear();
        _loadingAsyncDic.Clear();
        _loadedAssetDic.Clear();
        _unloadWaitList.Clear();
        _tempABUnitList.Clear();
#if UNITY_EDITOR
        fileNames.Clear();
#endif
    }

    public void Destroy()
    {
        OEF.Instance.Remove(this);
        //�ͷ�����δ�ͷŵ�bundle
        AssetBundle.UnloadAllAssetBundles(true);
        _loadAsyncWaitDic = null;
        _loadingAsyncDic = null;
        _loadedAssetDic = null;
        _unloadWaitList = null;
        _tempABUnitList = null;
#if UNITY_EDITOR
        fileNames = null;
#endif
    }

    private void Update()
    {
        UpdateWaitList();
        UpdateLoadingList();
        UpdateUnloadList();
    }
    /// <summary>
    /// ���µȴ����ض���
    /// </summary>
    private void UpdateWaitList()
    {
        if (_loadAsyncWaitDic.Count < 1)
        {
            return;
        }
        //ͬʱ������������������
        if (_loadingAsyncDic.Count > GameSetting.Instance.abLoadingMax)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _loadAsyncWaitDic.Values)
        {
            if (aBUnit.refCount > 0)
            {
                StartLoadAsync(aBUnit);
            }
            _tempABUnitList.Add(aBUnit);
            //������������
            if (_loadingAsyncDic.Count > GameSetting.Instance.abLoadingMax)
            {
                break;
            }
        }
        //�ӵȴ�������ȥ��
        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            _loadAsyncWaitDic.Remove(_tempABUnitList[i].bundleName);
        }
    }
    /// <summary>
    /// ���¼����ж���
    /// </summary>
    private void UpdateLoadingList()
    {
        if (_loadingAsyncDic.Count < 1)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _loadingAsyncDic.Values)
        {
            //���ö��������˲������
            if (aBUnit.dependLoadingCount < 1 && aBUnit.request != null && aBUnit.request.isDone == true)
            {
                _tempABUnitList.Add(aBUnit);
            }
        }
        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            AfterLoaded(_tempABUnitList[i]);
        }
    }
    /// <summary>
    /// ����ж�ض���
    /// </summary>
    private void UpdateUnloadList()
    {
        if (_unloadWaitList.Count < 1)
        {
            return;
        }
        _tempABUnitList.Clear();
        foreach (ABUnit aBUnit in _unloadWaitList.Values)
        {
            if (aBUnit.refCount > 0)//����ʹ�ã�ȡ��ж��
            {
                _tempABUnitList.Add(aBUnit);
            }
            else if (--aBUnit.unloadCountdown < 0)
            {
                aBUnit.Destroy();
                if (_loadedAssetDic.ContainsKey(aBUnit.bundleName))
                {
                    _loadedAssetDic.Remove(aBUnit.bundleName);
                }

                _tempABUnitList.Add(aBUnit);
            }
        }

        int length = _tempABUnitList.Count;
        for (int i = 0; i < length; i++)
        {
            _unloadWaitList.Remove(_tempABUnitList[i].bundleName);
        }
    }

    /// <summary>
    /// ��ȡһ��ABUnit
    /// </summary>
    /// <returns></returns>
    private ABUnit GetABUnit(string bundleName)
    {
        ABUnit rt = new ABUnit();
        rt.bundleName = bundleName;
        return rt;
    }
    /// <summary>
    /// �����ز����ƻ�ȡ��Ӧ��Bundle����
    /// </summary>
    /// <returns></returns>
    private string GetBundleName(string resPath)
    {
        string path = resPath.ToLower().Replace("\\", "/");
        int index = path.LastIndexOf("/");
        string abName = path.Substring(0, index) + "." + extension;
        return abName;
    }
    /// <summary>
    /// ��ȡһ���ز�
    /// ���AB��������ͬ������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iPath"></param>
    /// <returns></returns>
    public T GetRes<T>(string iPath) where T : Object
    {
#if UNITY_EDITOR
        if (fileNames != null && iPath != null)
        {
            if (fileNames.ContainsKey(iPath))
            {
                fileNames[iPath]++;
            }
            else
            {
                fileNames.Add(iPath, 1);
            }
        }
#endif
        if (iPath == null)
        {
            return null;
        }
        if (iPath.Length < 1)
        {
            return null;
        }
        return Resources.Load<T>(iPath);
        //string path = iPath.ToLower().Replace("\\", "/");
        //int index = path.LastIndexOf("/");
        //string abName = path.Substring(0, index) + "." + extension;
        //string fileName = path.Substring(index + 1);
        //AssetBundle ab = LoadAssetBundle(abName, path).assetBundle;
        //T t = ab.LoadAsset<T>(fileName);
        //return t;
    }
    /// <summary>
    /// ж���ز�
    /// </summary>
    public void UnloadRes(string resPath)
    {
        return;
        if (string.IsNullOrEmpty(resPath))
        {
            return;
        }
        ABUnit aBUnit = null;
        string bundleName = GetBundleName(resPath);

#if UNITY_EDITOR
        if (fileNames.ContainsKey(resPath))
        {
            fileNames[resPath]--;
        }
#endif


        if (_loadedAssetDic.ContainsKey(bundleName))
        {
            aBUnit = _loadedAssetDic[bundleName];
        }
        else if (_loadingAsyncDic.ContainsKey(bundleName))
        {
            aBUnit = _loadingAsyncDic[bundleName];
        }
        else if (_loadAsyncWaitDic.ContainsKey(bundleName))
        {
            aBUnit = _loadAsyncWaitDic[bundleName];
        }
        RemoveRefCount(aBUnit, resPath);
    }
    /// <summary>
    /// ж��ָ��ж�����͵��زģ�
    /// ֻж��Loaded�б��е��ز�
    /// </summary>
    /// <param name="disposType"></param>
    public void UnloadRes(ABUnit.DisposType disposType)
    {
        switch (disposType)
        {
            case ABUnit.DisposType.Restart://������Ϸ
                UnloadResRestart();
                break;
            case ABUnit.DisposType.ChangeScene://�л�����
            default:
                UnloadResChangeScene();
                break;
        }
    }
    /// <summary>
    /// ����ж�������ز�
    /// </summary>
    private void UnloadResRestart()
    {
        //��յȴ��б�
        _loadAsyncWaitDic.Clear();
        //�������б�����
        _loadingAsyncDic.Clear();
        //������������б�,ֱ��ж��
        foreach (ABUnit aBUnit in _loadedAssetDic.Values)
        {
            aBUnit.assetBundle.Unload(true);
        }
        _loadedAssetDic.Clear();
        //��������ж���б�
        foreach (ABUnit aBUnit in _unloadWaitList.Values)
        {
            aBUnit.assetBundle.Unload(true);

        }
#if UNITY_EDITOR
        fileNames.Clear();
#endif
        _unloadWaitList.Clear();
    }
    /// <summary>
    /// �л�����������Restart���زģ�ȫ������ж�ض���
    /// </summary>
    private void UnloadResChangeScene()
    {
        _loadAsyncWaitDic.Clear();
        foreach (ABUnit aBUnit in _loadingAsyncDic.Values)
        {
            if (aBUnit.disposType != ABUnit.DisposType.Restart)
            {
                AddUnloadWaitList(aBUnit);
            }
        }
        foreach (ABUnit aBUnit in _loadedAssetDic.Values)
        {
            if (aBUnit.disposType != ABUnit.DisposType.Restart)
            {
                AddUnloadWaitList(aBUnit);
            }
        }
#if UNITY_EDITOR
        fileNames.Clear();
#endif
    }
    /// <summary>
    /// ��ȡ�����б�
    /// û����������null
    /// </summary>
    private string[] GetDepends(string bundleName)
    {
        if (_dependList.ContainsKey(bundleName))
        {
            return _dependList[bundleName];
        }
        return null;
    }
    #region ���ü�����
    /// <summary>
    /// ����һ�����ü���
    /// ���������������һ�����ü���
    /// </summary>
    /// <param name="abName"></param>
    private void AddRefCount(ABUnit aBUnit, string resPath)
    {
        aBUnit.ChangeRefCount(1, resPath);
        if (aBUnit.dependList != null)
        {
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                AddRefCount(aBUnit.dependList[i], resPath);
            }
        }
    }
    /// <summary>
    /// ȥ��һ������
    /// </summary>
    /// <param name="aBUnit">��Ҫȥ�����õ�ABUnit</param>
    /// <param name="resPath">�����ͷŵ��زĵ�ַ</param>
    private void RemoveRefCount(ABUnit aBUnit, string resPath)
    {
        if (aBUnit == null)
        {
            return;
        }
        aBUnit.ChangeRefCount(-1, resPath);
        if (aBUnit.dependList != null)
        {
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                RemoveRefCount(aBUnit.dependList[i], resPath);
            }
        }
        if (aBUnit.refCount < 1)
        {
            AddUnloadWaitList(aBUnit);
        }
    }
    /// <summary>
    /// ���ӵ��ȴ�ж�ض���
    /// </summary>
    private void AddUnloadWaitList(ABUnit aBUnit)
    {
        aBUnit.refCount = 0;
        //��ʱж��
        aBUnit.unloadCountdown = GameSetting.Instance.abUnloadCountdownBase + _unloadWaitList.Count;
#if UNITY_EDITOR
        Debug.LogWarning("<color=#0000FFFF>����ȴ�ж���б�:</color>" + aBUnit.bundleName + "������ʱ��" + aBUnit.unloadCountdown);
#endif
        if (!_unloadWaitList.ContainsKey(aBUnit.bundleName))
        {
            _unloadWaitList.Add(aBUnit.bundleName, aBUnit);
        }
    }
    #endregion

    #region ͬ������
    /// <summary>
    /// ͬ������AssetBundle
    /// </summary>
    /// <returns></returns>
    private ABUnit LoadAssetBundle(string bundleName, string resPath)
    {
        ABUnit aBUnit;
        //AssetBundle.SetAssetBundleDecryptKey(HotfixSetting.Instance.bundleKey);
        if (_loadedAssetDic.ContainsKey(bundleName))//�Ѿ�����
        {
            aBUnit = _loadedAssetDic[bundleName];
            //��¼����
            AddRefCount(aBUnit, resPath);
            return aBUnit;
        }
        else if (_loadingAsyncDic.ContainsKey(bundleName))//���ڼ���
        {
            aBUnit = _loadingAsyncDic[bundleName];
            //��������ص�ʱ����������
            aBUnit.ChangeRefCount(1, resPath);
            //��������
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                LoadAssetBundle(aBUnit.dependList[i].bundleName, resPath);
            }
            AfterLoaded(aBUnit);
            return aBUnit;
        }
        else if (_loadAsyncWaitDic.ContainsKey(bundleName))//�ȴ�����
        {
            aBUnit = _loadAsyncWaitDic[bundleName];
            _loadAsyncWaitDic.Remove(bundleName);
            //��������ص�ʱ����������
            aBUnit.ChangeRefCount(1, resPath);
            int length = aBUnit.dependList.Count;
            for (int i = 0; i < length; i++)
            {
                LoadAssetBundle(aBUnit.dependList[i].bundleName, resPath);
            }
            AddRefCount(aBUnit, resPath);
            string path1;
            if (GameSetting.Instance.IsResInPatch(bundleName))
            {
                path1 = GameSetting.Instance.abPatchAddress + bundleName;
            }
            else
            {
                path1 = GameSetting.Instance.abAddressLocal + bundleName;
            }
            AssetBundle ab1 = AssetBundle.LoadFromFile(path1);
            aBUnit.assetBundle = ab1;
            AfterLoaded(aBUnit);
            return aBUnit;
        }
        aBUnit = GetABUnit(bundleName);
        //��¼����
        AddRefCount(aBUnit, resPath);
        string path;
        if (GameSetting.Instance.IsResInPatch(bundleName))
        {
            path = GameSetting.Instance.abPatchAddress + bundleName;
        }
        else
        {
            path = GameSetting.Instance.abAddressLocal + bundleName;
        }
        AssetBundle ab = AssetBundle.LoadFromFile(path);
        aBUnit.assetBundle = ab;
        //��������
        LoadDependency(aBUnit, resPath);
        AfterLoaded(aBUnit);
        //AssetBundle.SetAssetBundleDecryptKey(null);
        return aBUnit;
    }
    /// <summary>
    /// ����������bundle
    /// </summary>
    /// <param name="key"></param>
    private void LoadDependency(ABUnit aBUnit, string resPath)
    {
        string[] abs = GetDepends(aBUnit.bundleName);
        if (abs != null)
        {
            int length = abs.Length;
            for (int i = 0; i < length; i++)
            {
                aBUnit.dependList.Add(LoadAssetBundle(abs[i], resPath));
            }
        }
    }
    #endregion

    #region �첽����
    /// <summary>
    /// �첽����һ����Դ
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="callback"></param>
    public void LoadAsync(string resPath, System.Action callback)
    {
        string bundleName = GetBundleName(resPath);
        LoadAssetBundleAsync(bundleName, resPath, callback);
    }
    /// <summary>
    /// �첽�����زģ����ӵ��ȴ�����
    /// </summary>
    /// <param name="bundleName"></param>
    private ABUnit LoadAssetBundleAsync(string bundleName, string resPath, System.Action callback)
    {
        //���ӵ��ȴ�����
        ABUnit aBUnit = GetABUnit(bundleName);
        AddRefCount(aBUnit, resPath);
        //�ȼ�������
        LoadDependencyAsync(aBUnit, resPath);
        _loadAsyncWaitDic.Add(bundleName, aBUnit);
        return aBUnit;
    }
    /// <summary>
    /// ����������bundle
    /// </summary>
    /// <param name="key"></param>
    private void LoadDependencyAsync(ABUnit aBUnit, string resPath)
    {
        string[] abs = GetDepends(aBUnit.bundleName);
        int length = abs.Length;
        for (int i = 0; i < length; i++)
        {
            aBUnit.dependList.Add(LoadAssetBundleAsync(abs[i], resPath, aBUnit.AfterDependLoaded));
        }
    }
    /// <summary>
    /// �ӵȴ������л�ȡ��ʼ����
    /// </summary>
    /// <param name="aBUnit"></param>
    private void StartLoadAsync(ABUnit aBUnit)
    {
        string path = GameSetting.Instance.abAddressLocal + aBUnit.bundleName;
        aBUnit.request = AssetBundle.LoadFromFileAsync(path);
        _loadingAsyncDic.Add(aBUnit.bundleName, aBUnit);
    }
    #endregion
    /// <summary>
    /// �������Ҫִ�еķ���
    /// </summary>
    private void AfterLoaded(ABUnit aBUnit)
    {
#if UNITY_EDITOR && RESLOADER
        Debug.Log("<color=#CC6411ff>�زĹ���������</color>" + aBUnit.bundleName);
#endif
        if (aBUnit.request != null)
        {
            aBUnit.assetBundle = aBUnit.request.assetBundle;//�첽���صĻ�ת��ͬ������
        }
        if (_loadingAsyncDic.ContainsKey(aBUnit.bundleName))
        {
            _loadingAsyncDic.Remove(aBUnit.bundleName);
        }
        _loadedAssetDic.Add(aBUnit.bundleName, aBUnit);
        //���Ƶ�Loaded������ִ��callBack�����ܻص��л���ص�ǰ�زģ�˳�򷴹����ʹ���Ĳ�����غ����ز���
        if (aBUnit.callBackList.Count > 0)
        {
            System.Action callBack;
            int length = aBUnit.callBackList.Count;
            for (int i = 0; i < length; i++)
            {
                callBack = aBUnit.callBackList[i];
                if (callBack != null)
                {
                    callBack();
                }
            }
            aBUnit.callBackList.Clear();
        }
    }
}
