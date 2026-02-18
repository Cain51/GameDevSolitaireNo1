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
	public GameObject buttonPrefab;  // 按钮预制件
	public Transform buttonContainer; // 存放按钮的容器
	public DialogGraph dialogGraph;  // 指向在XNode编辑器中创建的DialogGraph
	private DialogNode currentNode;
	private int currentTextIndex = 0;  // 当前展示的对话段落索引
	public float typingSpeed = 0.05f;  // 打字机效果每个字符的显示间隔
	private Coroutine typingCoroutine; // 保存打字机效果的协程
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

		// 清空现有按钮
		foreach (Transform child in buttonContainer)
		{
			Destroy(child.gameObject);
		}

		// 停止当前的打字机协程（如果有的话）
		if (typingCoroutine != null)
		{
			StopCoroutine(typingCoroutine);
			typingCoroutine = null;
        }

     
        // 逐段展示对话
        if (currentTextIndex < currentNode.dialogText.Count)
		{
			LoadReward();
			string dialog = currentNode.dialogText[currentTextIndex];
			speak.ConvertAndSpeak(dialog);
			// 启动新的打字机效果协程
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
                // 创建选项按钮
                for (int i = 0; i < currentNode.choices.Count; i++)
				{
					CreateChoiceButton(currentNode.choices[i].choiceText, i);
				}
			}

		}
	}

	// 打字机效果的协程
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

		lbl_content.text = "";  // 先清空文本框
		foreach (char letter in sentence.ToCharArray())
		{
			lbl_content.text += letter;  // 逐字显示
			yield return new WaitForSeconds(typingSpeed);  // 每个字符的显示时间间隔
		}
		typingCoroutine = null; // 完成打字效果后将协程引用置为空


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
		// 设置当前物体的 Layer
		objTransform.gameObject.layer = layer;

		// 递归设置所有子物体的 Layer
		foreach (Transform child in objTransform)
		{
			SetLayer(child, layer);
		}
	}
	// 创建选择按钮
	private void CreateChoiceButton(string choiceText, int index)
	{
		GameObject buttonObject = Instantiate(buttonPrefab, buttonContainer);
		buttonObject.SetActive(true);
		Button button = buttonObject.GetComponent<Button>();
		button.GetComponentInChildren<Text>().text = choiceText;  // 设置按钮文本

		// 设置按钮点击事件
		button.onClick.AddListener(() => OnChoiceButtonClicked(index));
	}

	// 按钮点击事件处理
	private void OnChoiceButtonClicked(int index)
	{
		DialogChoice choice = currentNode.choices[index];
		GameManager gm = CBus.Instance.GetManager(ManagerName.GameManager) as GameManager;
		if (choice.IsChoiceAvailable(gm.bag) == false)
		{
			//弹tips
			TipManager.Tip("条件不足");
			return;
		}
		// 根据玩家的选择跳转到对应的节点
		currentNode = currentNode.GetOutputPort("nextNode " + index).Connection.node as DialogNode;
		currentTextIndex = 0; // 重置文本索引
		DisplayCurrentNode(); // 显示下一个对话节点
	}

	public void NextSegmentOrChooseOption()
	{
		//// 如果打字机效果未完成，立即完成当前文本显示
		//if (typingCoroutine != null)
		//{
		//	if(currentNode.dialogText.Count<= currentTextIndex){
		//		lbl_content.text = currentNode.dialogText[currentTextIndex - 1]; // 立即显示完整文本
		//	}
  //          StopCoroutine(typingCoroutine); // 停止打字机协程
  //          typingCoroutine = null;  // 打字机协程已结束

		//	return;  // 返回，等待玩家再次点击继续下一段
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
			reward += $"获得:{ca.name}x{1} ";
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
			reward += $"损失:{ca.name}x{1} ";
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

		
		//是否打断
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
