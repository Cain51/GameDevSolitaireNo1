using System;

namespace RG.Basic
{
    public class LinkHead<T>
    {
        /// <summary>
        /// 头节点
        /// </summary>
        public LinkNode<T> first;
        /// <summary>
        /// 尾结点
        /// </summary>
        public LinkNode<T> last;
        /// <summary>
        /// 节点总数
        /// </summary>
        public int count;

        public LinkHead()
        {
            count = 0;
        }
        /// <summary>
        /// 去除第一个节点并返回
        /// </summary>
        /// <returns>第一个节点</returns>
        public LinkNode<T> RemoveFirst()
        {
            LinkNode<T> rt = first;
            if (rt != null)
            {
                Remove(rt);
            }
            return rt;
        }
        /// <summary>
        /// 去除最后一个节点并返回
        /// </summary>
        /// <returns>最后一个节点</returns>
        public LinkNode<T> RemoveLast()
        {
            LinkNode<T> rt = last ;
            if (rt != null)
            {
                Remove(rt);
            }
            return rt;
        }
        /// <summary>
        /// 移除一个节点
        /// 默认节点是在这个Link中的，没有做相关判定
        /// </summary>
        public void Remove(LinkNode<T> node)
        {
            if(count < 2)//last == first
            {
                first = last = null;
            }
            else if(first == node)
            {
                first = node.next;
                if(first != null)
                {
                    first.pre = null;
                }
                node.next = null;
            }
            else if(last == node)
            {
                last = node.pre;
                if(last != null)
                {
                    last.next = null;
                }
                node.pre = null;
            }
            else
            {
                node.pre.next = node.next;
                node.next.pre = node.pre;
                node.pre = null;
                node.next = null;
            }
            --count;
        }
        /// <summary>
        /// 移除目标节点以前的全部节点
        /// 目标节点成为头结点
        /// 重新计算count
        /// </summary>
        public void RemoveAllNodesBefore(LinkNode<T> iNode)
        {
            int removeCount = 0;
            LinkNode<T> node = iNode.pre;
            LinkNode<T> pre = null;
            while (node != null)
            {
                removeCount++;
                pre = node.pre;
                node.Destroy();
                node = pre;
            }
            iNode.pre = null;
            first = iNode;
            count -= removeCount;
        }
        /// <summary>
        /// 移除目标节点以后的全部节点
        /// 目标节点成为尾结点
        /// 重新计算count
        /// </summary>
        public void RemoveAllNodesAfter(LinkNode<T> iNode)
        {
            int removeCount = 0;
            LinkNode<T> node = iNode.next;
            LinkNode<T> next = null;
            while (node != null)
            {
                removeCount++;
                next = node.next;
                node.Destroy();
                node = next;
            }
            iNode.next = null;
            last = iNode;
            count -= removeCount;
        }
        /// <summary>
        /// 向队首添加一个节点
        /// </summary>
        /// <param name="item">要添加的节点内容</param>
        /// <param name="recycle">item对应的</param>
        /// <returns>新加入的node</returns>
        public LinkNode<T> AddFirst(T item, Action recycle = null)
        {
            LinkNode<T> node = new LinkNode<T>();
            node.item = item;
            node.recycle = recycle;
            AddFirst(node);
            return node;
        }
        /// <summary>
        /// 向队尾添加一个节点
        /// </summary>
        /// <param name="item">要添加的节点内容</param>
        /// <param name="recycle">item对应的</param>
        /// <returns>新加入的node</returns>
        public LinkNode<T> AddLast(T item, Action recycle = null)
        {
            LinkNode<T> node = new LinkNode<T>();
            node.item = item;
            node.recycle = recycle;
            AddLast(node);
            return node;
        }
        /// <summary>
        /// 向指定位置添加一个节点
        /// </summary>
        /// <param name="item">要添加的节点内容</param>
        /// <param name="recycle">item对应的</param>
        /// <param name="index">索引，大于等于count直接AddLast</param>
        /// <returns>新加入的node</returns>
        public LinkNode<T> AddByIndex(T item, Action recycle = null,int index = 0)
        {
            LinkNode<T> node = new LinkNode<T>();
            node.item = item;
            node.recycle = recycle;
            AddByIndex(node, index);
            return node;
        }
        /// <summary>
        /// 向队首添加一个节点，这个节点是已经构建好的
        /// </summary>
        public void AddFirst(LinkNode<T> node)
        {
            node.head = this;
            if (count == 0)
            {
                first = node;
                last = node;
            }
            else
            {
                node.next = first;
                node.pre = null;
                first.pre = node;
                first = node;
            }
            ++count;
        }
        /// <summary>
        /// 向队尾添加一个节点
        /// </summary>
        public void AddLast(LinkNode<T> node)
        {
            node.head = this;
            if (count == 0)
            {
                first = node;
                last = node;
            }
            else
            {
                node.pre = last;
                node.next = null;
                last.next = node;
                last = node;
            }
            ++count;
        }
        /// <summary>
        /// 向指定位置添加一个节点
        /// </summary>
        /// <param name="item">要添加的节点内容</param>
        /// <param name="recycle">item对应的</param>
        /// <param name="index">索引，大于count直接AddLast</param>
        /// <returns>新加入的node</returns>
        public void AddByIndex(LinkNode<T> node, int index = 0)
        {
            if (index == 0)
            {
                AddFirst(node);
            }
            else if (index >= count)
            {
                AddLast(node);
            }
            else
            {
                node.head = this;

                LinkNode<T> tNode;
                //根据Index从前或者从后开始遍历添加
                if (index > count / 2)
                {
                    tNode = last;
                    int tCount = count;
                    while (--tCount > index)
                    {
                        tNode = tNode.pre;
                    }
                    //放在这个节点前面
                    node.next = tNode;
                    node.pre = tNode.pre;
                    node.pre.next = node;
                    tNode.pre = node;
                }
                else
                {
                    tNode = first;
                    while (--index > 0)
                    {
                        tNode = tNode.next;
                    }
                    //放在这个节点后面
                    node.next = tNode.next;
                    node.pre = tNode;
                    node.next.pre = node;
                    tNode.next = node;
                }
                ++count;
            }
        }
        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            LinkNode<T> node = first;
            LinkNode<T> next;
            while (node != null)
            {
                next = node.next;
                node.Destroy();
                node = next;
            }
            first = null;
            last = null;
            count = 0;
        }
    }
}