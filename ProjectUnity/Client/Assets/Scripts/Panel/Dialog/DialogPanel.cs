using RG.Zeluda;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : PanelBase
{
	public Transform tran_node;
	public Text lbl_name;
	public GameObject go_name;
	public Text lbl_content;
	public GameObject buttonPrefab;  // ��ťԤ�Ƽ�
	public Transform buttonContainer; // ��Ű�ť������
	public DialogGraph dialogGraph;  // ָ����XNode�༭���д�����DialogGraph
	private DialogNode currentNode;
	private int currentTextIndex = 0;  // ��ǰչʾ�ĶԻ���������
	public float typingSpeed = 0.05f;  // ���ֻ�Ч��ÿ���ַ�����ʾ���
	private Coroutine typingCoroutine; // ������ֻ�Ч����Э��
	private string graphName;
	private string m_sentence;
	public SpeakPronounce speak;
	private bool isSimple = false;
	public Action OnCallback;
	public void StartDialog(DialogNode startNode)
	{
		lbl_name.text = name;
		currentNode = startNode;
		currentTextIndex = 0;
		isSimple = false;
		DisplayCurrentNode();
		AudioManager.Inst.Play("BGM/对话框展开");
	}
	public void StartDialog(string gn)
	{
		OnCallback = null;
		graphName = gn;
		DialogGraph dn = Resources.Load<DialogGraph>($"Dialog/{gn}");
		if (dn == null) { return; }
		currentNode = dn.nodes[0] as DialogNode;
		currentTextIndex = 0;
		isSimple = false;
		DisplayCurrentNode();
		AudioManager.Inst.Play("BGM/对话框展开");
	}
	public void ShowSimple(string name, string sentence)
	{
		lbl_name.text = name;
		isSimple = true;
        m_sentence = sentence;
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
		AudioManager.Inst.Play("BGM/对话框展开");
		speak.ConvertAndSpeak(sentence);
	}
	private void DisplayCurrentNode()
	{
		if (currentNode == null) return;

		// ������а�ť
		foreach (Transform child in buttonContainer)
		{
			Destroy(child.gameObject);
		}

		// ֹͣ��ǰ�Ĵ��ֻ�Э�̣�����еĻ���
		if (typingCoroutine != null)
		{
			StopCoroutine(typingCoroutine);
			typingCoroutine = null;
        }

     
        // ���չʾ�Ի�
        if (currentTextIndex < currentNode.dialogText.Count)
		{
			LoadReward();
			string dialog = currentNode.dialogText[currentTextIndex];
			speak.ConvertAndSpeak(dialog);
			// �����µĴ��ֻ�Ч��Э��
			typingCoroutine = StartCoroutine(TypeSentence(dialog));

		}
		else
		{
			int cnt = currentNode.choices.Count;
			if (cnt == 0)
			{
				
				Close();
				OnDialogOver();
				PlayerPrefs.SetString($"{graphName}", currentNode.endingDescription);

			}
			else
			{
                LoadScene();
                // ����ѡ�ť
                for (int i = 0; i < currentNode.choices.Count; i++)
				{
					CreateChoiceButton(currentNode.choices[i].choiceText, i);
				}
			}

		}
	}

	// ���ֻ�Ч����Э��
	private IEnumerator TypeSentence(string sentence)
	{
		if (currentNode.speakerid != 0)
		{
			CharacterFactory cf = CBus.Instance.GetFactory(FactoryName.CharacterFactory) as CharacterFactory;
			CharacterCA cca = cf.GetCA(currentNode.speakerid) as CharacterCA;
			foreach (Transform c in tran_node)
			{
				Destroy(c.gameObject);
			}
			GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(cca.path));
			obj.transform.SetParent(tran_node);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			if (go_name.activeSelf == false)
			{
				go_name.SetActive(true);
			}
			lbl_name.text = cca.name;

			int layerIndex = LayerMask.NameToLayer("Dialog");
			SetLayer(obj.transform, layerIndex);
		}
		else
		{
			if (go_name.activeSelf == true)
			{
				go_name.SetActive(false);
			}
		}

		lbl_content.text = "";  // ������ı���
		foreach (char letter in sentence.ToCharArray())
		{
			lbl_content.text += letter;  // ������ʾ
			yield return new WaitForSeconds(typingSpeed);  // ÿ���ַ�����ʾʱ����
		}
		typingCoroutine = null; // ��ɴ���Ч����Э��������Ϊ��


    }
	public void LoadScene() {
        if (currentNode.scene != null && currentNode.scene != string.Empty)
        {
            SceneLoadManager slm = CBus.Instance.GetManager(ManagerName.SceneLoadManager) as SceneLoadManager;
            slm.Load(currentNode.scene);
            if (currentNode.prefab != null && currentNode.prefab != string.Empty)
            {
                slm.LoadSimplePrefab(currentNode.prefab);
            }
        }
    }
	void SetLayer(Transform objTransform, int layer)
	{
		// ���õ�ǰ����� Layer
		objTransform.gameObject.layer = layer;

		// �ݹ���������������� Layer
		foreach (Transform child in objTransform)
		{
			SetLayer(child, layer);
		}
	}
	// ����ѡ��ť
	private void CreateChoiceButton(string choiceText, int index)
	{
		GameObject buttonObject = Instantiate(buttonPrefab, buttonContainer);
		buttonObject.SetActive(true);
		Button button = buttonObject.GetComponent<Button>();
		button.GetComponentInChildren<Text>().text = choiceText;  // ���ð�ť�ı�

		// ���ð�ť����¼�
		button.onClick.AddListener(() => OnChoiceButtonClicked(index));
	}

	// ��ť����¼�����
	private void OnChoiceButtonClicked(int index)
	{
		DialogChoice choice = currentNode.choices[index];
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (choice.IsChoiceAvailable(gm.bag) == false)
		{
			//��tips
			TipManager.Tip("��������");
			return;
		}
		// ������ҵ�ѡ����ת����Ӧ�Ľڵ�
		currentNode = currentNode.GetOutputPort("nextNode " + index).Connection.node as DialogNode;
		currentTextIndex = 0; // �����ı�����
		DisplayCurrentNode(); // ��ʾ��һ���Ի��ڵ�
	}

	public void NextSegmentOrChooseOption()
	{
		//// ������ֻ�Ч��δ��ɣ�������ɵ�ǰ�ı���ʾ
		//if (typingCoroutine != null)
		//{
		//	if(currentNode.dialogText.Count<= currentTextIndex){
		//		lbl_content.text = currentNode.dialogText[currentTextIndex - 1]; // ������ʾ�����ı�
		//	}
  //          StopCoroutine(typingCoroutine); // ֹͣ���ֻ�Э��
  //          typingCoroutine = null;  // ���ֻ�Э���ѽ���

		//	return;  // ���أ��ȴ�����ٴε��������һ��
		//}
		if (typingCoroutine != null) { return; }
		if (isSimple == true) { Close(); return; }

		currentTextIndex++;
		DisplayCurrentNode();
	}
	public void LoadReward() {
		GameManager gameManager = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		string reward = string.Empty;
		foreach (var item in currentNode.rewards)
		{
			AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
			AssetCA ca = assetFactory.GetCA(item.k) as AssetCA;
			reward += $"���:{ca.name}x{1} ";
			if (gameManager.bag.ContainsKey(item.k))
			{
				gameManager.bag[item.k] += item.v;
			}
			else
			{
				gameManager.bag.Add(item.k, item.v);
			}
		}
		foreach (var item in currentNode.cost)
		{
			AssetFactory assetFactory = CBus.Instance.GetFactory(FactoryName.AssetFactory) as AssetFactory;
			AssetCA ca = assetFactory.GetCA(item.k) as AssetCA;
			reward += $"��ʧ:{ca.name}x{1} ";
			if (gameManager.bag.ContainsKey(item.k))
			{
				gameManager.bag[item.k] -= item.v;
			}
			else
			{
				gameManager.bag.Add(item.k, -item.v);
			}
		}
		
		if (reward != string.Empty)
		{
			UIManager um = CBus.Instance.GetManager(ManagerName.UIManager) as UIManager;
			MainPanel main = um.GetPanel("MainPanel") as MainPanel;
			main.RefreshBeg();
			TipManager.Tip(reward);
		}
	}
	public void OnDialogOver()
	{
        LoadScene();
		//if (currentNode.rewards.Count == 0) { return; }

		
		//�Ƿ���
		if (currentNode.eventid != 0) {
			EventManager eventManager = CBus.Instance.GetManager(ManagerName.EventManager) as EventManager;
			eventManager.TriggerEvent(currentNode.eventid);
		}
		
		OnCallback?.Invoke();
	}
	public override void Open()
	{
		gameObject.SetActive(true);
		base.Open();
	}
	public override void Close()
	{
		gameObject.SetActive(false);
		base.Close();
	}
}
