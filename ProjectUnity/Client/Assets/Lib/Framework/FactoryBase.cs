
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FactoryBase
{
	public virtual int GetFactoryCode() { return 0; }
	/// <summary>
	/// <id,CA>,第一次使用时添加
	/// </summary>
	protected Dictionary<int, CABase> _caDic;
	/// <summary>
	/// 内存池<id,产品>
	/// </summary>
	private Dictionary<int, LinkedList<Product>> _memoryPool;
	/// <summary>
	/// 内存池当前容量
	/// </summary>
	private int _memoryPoolCount = 0;
	/// <summary>
	/// 内存池单类型容量上限
	/// </summary>
	protected int _memoryPoolCapacityEach = 0;
	/// <summary>
	/// 内存池总容量上限
	/// </summary>
	protected int _memoryPoolCapacityTotal = 0;
	/// <summary>
	/// 当前生效产品列表
	/// </summary>
	private List<Product> _productList;
	/// <summary>
	/// 工厂清理标记
	/// </summary>
	private bool _isClear;
	/// <summary>
	/// 切换场景的时候需要进行的操作
	/// </summary>
	public OperationType changeSceneOperation;
	/// <summary>
	/// CBUS引用
	/// </summary>
	public CBus _cbus;
	public FactoryBase()
	{
		_caDic = new Dictionary<int, CABase>();
		_memoryPool = new Dictionary<int, LinkedList<Product>>();
		_productList = new List<Product>();
		Init();
		changeSceneOperation = OperationType.None;
	}
	protected virtual void Init()
	{
		_cbus = CBus.Instance;
		_memoryPoolCapacityEach = 0;
		_memoryPoolCapacityTotal = 0;
	}
	public virtual void InitParams()
	{
		_isClear = false;
	}
	/// <summary>
	/// 生产产品
	/// </summary>
	/// <param name="id">产品ID</param>
	/// <returns></returns>
	public virtual Product Produce(int id)
	{
		Product rt = null;
		rt = Reuse(id);
		if (rt == null)
		{
			rt = CreateClass(id);
		}
		OEF.Instance.debuggingObject = rt;
		RegProduct(rt);//注册管理当前生效产品

		rt.produceFrame = CBus.Instance.currentFrame;
		rt.produceIndex = CBus.Instance.produceIndex;
		rt.uniqueId = id.ToString() + rt.produceFrame.ToString() + rt.produceIndex.ToString();
		CBus.Instance.produceIndex++;

		return rt;
	}
	/// <summary>
	/// 创建对象实例
	/// </summary>
	/// <param name="id">对象的id</param>
	/// <returns></returns>
	protected virtual Product CreateClass(int id)
	{
		return null;
	}
	/// <summary>
	/// 获取指定ID的CA
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public virtual CABase GetCA(int id, bool isRefresh = false)
	{
		CABase rt = null;
		if (!_caDic.ContainsKey(id) || isRefresh)
		{
			//从未使用过，获取组装新的CA
			CreateCA(id);
		}
		if (_caDic.ContainsKey(id))
		{
			//可能工厂不包含对应模型导致返回为空
			rt = _caDic[id];
		}
		return rt;
	}
	public virtual void CreateCA(int id)
	{
		CABase ca = new CABase();
		ca.factory = this;
		_caDic[id] = ca;
	}
	/// <summary>
	/// 从内存池中重用
	/// </summary>
	/// <param name="id">重用对象id</param>
	/// <returns></returns>
	protected Product Reuse(int id)
	{
		Product rt = null;
		if (_memoryPool.ContainsKey(id))
		{
			LinkedListNode<Product> node = _memoryPool[id].First;
			if (node != null)
			{
				//回收之后下一帧才可以使用
				if (node.Value.lastRecycleFrame < _cbus.currentFrame)
				{
					rt = node.Value;
					rt.Reuse();
					_memoryPool[id].Remove(node);
					--_memoryPoolCount;
				}
			}
		}
		return rt;

	}
	/// <summary>
	/// 生成所有的CA
	/// </summary>
	public virtual void PreLoadAllCA()
	{
		int startID = GetFactoryCode() * 10000 + 1;
		while (true)
		{
			CABase ca = GetCA(startID);
			if (ca == null || ca.id == 0 ) { break; }
			startID++;
		}
	}

	public virtual CABase[] GetAllCA()
	{
		PreLoadAllCA();
		int len = _caDic.Count;
		CABase[] rel = new CABase[len];
		int idx = 0;
		foreach (var item in _caDic)
		{
			rel[idx] = item.Value;
			idx++;
		}
		return rel;
	}
	/// <summary>
	/// 清理所有当前生效的产品
	/// </summary>
	private void ClearProducts()
	{
		int length = _productList.Count;
		while (--length > -1)
		{
			_productList[length].Recycle();//注销在recycle中做
		}
	}
	//注册当前生效产品
	private void RegProduct(Product product)
	{
		_productList.Add(product);
	}
	//注销当前生效产品
	private void UnregProduct(Product product)
	{
		_productList.Remove(product);
	}
	/// <summary>
	/// 清理释放
	/// </summary>
	public virtual void Clear()
	{
		_isClear = true;
		//当前产品清理
		ClearProducts();
		//内存池清理
		foreach (LinkedList<Product> link in _memoryPool.Values)
		{
			link.Clear();
		}
		// _caDic.Clear();
		_memoryPool.Clear();
		_memoryPoolCount = 0;
	}
	public virtual void ClearMemoryPool()
	{
		_memoryPool.Clear();
		_memoryPoolCount = 0;
	}
	/// <summary>
	/// 销毁
	/// </summary>
	public virtual void Destroy()
	{
		Clear();
		_productList = null;
		_memoryPool = null;
	}
	/// <summary>
	/// 内存池
	/// </summary>
	/// <param name="product">保存的产品对象</param>
	/// <param name="recycle">内存池链表节点的Recycle方法，一般使用产品的Destroy方法</param>
	public void Recycle(Product product, Action recycle = null)
	{
		//注销当前生效产品
		UnregProduct(product);
		//工厂被销毁，直接销毁产品
		if (_isClear)
		{
			product.Destroy();
			return;
		}
		//不需要内存池
		if (_memoryPoolCapacityTotal < 1)
		{
			product.Destroy();
			return;
		}
		int id = product.ca.id;
		if (!_memoryPool.ContainsKey(id))
		{
			_memoryPool[id] = new LinkedList<Product>();
		}
		//这个类型的已经达到单类型数量上限
		if (_memoryPool[id].Count >= _memoryPoolCapacityEach)
		{
			product.Destroy();
			return;
		}
		//超出存储上限每个类型-1
		if (_memoryPoolCount > _memoryPoolCapacityTotal)
		{
			foreach (LinkedList<Product> link in _memoryPool.Values)
			{
				if (link.Count > 0)
				{
					//去除最后一个，最后一个最有可能是当前帧回收的，要到下一帧才可以用
					link.Last.Value.Destroy();
					link.RemoveLast();
					--_memoryPoolCount;
				}
			}
		}
		_memoryPool[product.ca.id].AddLast(product);
		++_memoryPoolCount;
	}
}
