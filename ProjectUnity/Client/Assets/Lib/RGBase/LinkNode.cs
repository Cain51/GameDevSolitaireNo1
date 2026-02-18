using System;
/// <summary>
/// 所有操作都在LinkHead中
/// </summary>
namespace RG.Basic
{
    public class LinkNode<T>
    {
        /// <summary>
        /// 前一个节点
        /// </summary>
        public LinkNode<T> pre;
        /// <summary>
        /// 后一个节点
        /// </summary>
        public LinkNode<T> next;
        /// <summary>
        /// 表头
        /// </summary>
        public LinkHead<T> head;
        /// <summary>
        /// 节点内容
        /// </summary>
        public T item;
        /// <summary>
        /// item对应的回收方法
        /// </summary>
        public Action recycle;

        /// <summary>
        /// 准备销毁
        /// 标记之后在下一帧统一销毁
        /// 避免连锁销毁导致后面的节点读不到
        /// </summary>
        public bool isReadyToDestroy = false;
        /// <summary>
        /// 把自己从链表中去除
        /// </summary>
        public void Remove(bool isNeedDestroy = true)
        {
            head.Remove(this);
            if (isNeedDestroy)
            {
                Destroy();
            }
        }
        /// <summary>
        /// 移除自身以前的全部节点
        /// 自己成为头结点
        /// 重新计算count
        /// </summary>
        public void RemoveAllNodesBefore()
        {
            head.RemoveAllNodesBefore(this);
        }
        /// <summary>
        /// 移除自身以后的全部节点
        /// 自己成为尾结点
        /// 重新计算count
        /// </summary>
        public void RemoveAllNodesAfter()
        {
            head.RemoveAllNodesAfter(this);
        }
        public void Destroy()
        {
            pre = null;
            next = null;
            head = null;
            if (recycle != null)
            {
                recycle.Invoke();
            }
            item = default;
        }
    }
}