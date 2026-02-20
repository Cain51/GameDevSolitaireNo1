using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RG.Zeluda
{
    public class BookManager : ManagerBase
    {
        public List<int> bookList = new List<int>();
        public List<int> spBookList = new List<int>();

        private TipManager tipManager;
        protected override void Init()
        {
            tipManager = CBus.Instance.GetManager(ManagerName.TipManager) as TipManager;
            //bookList = new List<int>(DataCenter.bookAry);
            base.Init();
        }
        public override void InitParams()
        {
            base.InitParams();
        }
        /// <summary>
        /// ÿ���������ʶ���Ҫ�������
        /// </summary>
        /// <param name="id"></param>
        public void AddBook(int id)
        {
            if (id == 0) { return; }
            if (bookList.Contains(id) || spBookList.Contains(id)) { return; }
            bookList.Add(id);
            AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
            AssetCA ca = assetFactory != null ? assetFactory.GetCA(id) as AssetCA : null;
            if (ca != null)
            {
                TipManager.Tip($"解锁图鉴：{ca.name}");
            }
        }
     
        public void RefreshAsset()
        {
            AssetManager am = CBus.Instance.GetManager(ManagerName.AssetManager) as AssetManager;
            if (am == null) { return; }
            foreach (var kv in am.assetDic)
            {
                if (kv.Value <= 0) { continue; }
                AddBook(kv.Key);
            }
        }
    }
}
