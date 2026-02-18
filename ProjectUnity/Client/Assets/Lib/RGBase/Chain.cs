using System.Collections;
using System;
using RG.Basic;

namespace RG.Basic
{
    public class ChainNode<T>
    {
        public T val;
        public ChainNode<T> next;
        public ChainNode<T> pre;

        private ChainNode() { }
        static ChainNode<T> cache = null;
        public static ChainNode<T> Create()
        {
            if (cache == null) return new ChainNode<T>();
            else
            {
                var x = cache;
                cache = cache.next;
                x.pre = x.next = null;
                return x;
            }
        }

        public static void Destroy(ChainNode<T> cn)
        {
            cn.val = default(T);
            cn.next = null;
            cn.pre = null;
            cn.next = cache;
            cache = cn;
        }
    }

    public class Chain<T>
    {
        ChainNode<T> head;
        ChainNode<T> tail;
        int count;
        ChainNode<T> iterator;
        
        public int Count { get { return count; } }

        public ChainNode<T> Head { get { return head; } }
        public ChainNode<T> Tail { get { return tail; } }

        public bool IteratorIsNull()
        {
            return iterator == null;
        }
        public T Current
        {
            get { return iterator == null ? default(T) : iterator.val; }
        }

        public void Clear()
        {
            ChainNode<T> k = head;
            while (k != null)
            {
                ChainNode<T> next = k.next;
                k.next = null;
                k.pre = null;
                k = next;
            }
            head = null;
            tail = null;
            count = 0;
        }

        public void Add(T val)
        {
            ChainNode<T> k = ChainNode<T>.Create();
            k.val = val;
            k.next = null;
            if (tail != null)
            {
                k.pre = tail;
                tail.next = k;
                tail = k;
            }
            else
            {
                head = k;
                tail = k;
                k.pre = null;
            }
            count++;
        }

        public void AddAtFirst(T val)
        {
            ChainNode<T> k = ChainNode<T>.Create();
            k.val = val;
            k.pre = null;
            if (head != null)
            {
                k.next = head;
                head.pre = k;
                head = k;
            }
            else
            {
                head = k;
                tail = k;
                k.next = null;
            }
            count++;
        }

        public bool MoveHead() { iterator = head; return iterator != null; }

        public bool MoveNext() { if (iterator != null) iterator = iterator.next;return iterator != null; }

        public void RemoveX(Predicate<T> _Pre)
        {
            ChainNode<T> k = head;
            while (k != null)
            {
                ChainNode<T> next = k.next;
                if (_Pre(k.val)) RemoveX(k);
                k = next;
            }
        }

        /// <summary>
        /// 警告：别的链表的东西别拿过来哈，出问题了不管
        /// </summary>
        /// <param name="chainNode"></param>
        public void RemoveX(ChainNode<T> chainNode)
        {
            if (head == null || chainNode == null) return;
            if (chainNode == tail) tail = chainNode.pre;
            if(chainNode.pre == null)
            {
                head = chainNode.next;
                if (head != null) head.pre = null;
                count--;
            }
            else
            {
                chainNode.pre.next = chainNode.next;
                if (chainNode.next != null) chainNode.next.pre = chainNode.pre;
                count--;
            }
            chainNode.next = null;
            chainNode.pre = null;
            chainNode.val = default(T);
            //DestoryNode(chainNode);
            ChainNode<T>.Destroy(chainNode);
        }

        public void RemoveCurrent()
        {
            if (iterator == null) return;
            ChainNode<T> k = iterator;
            MoveNext();
            RemoveX(k);
        }

        public T PopFirst()
        {
            if (head == null) return default(T);
            if (tail == head) tail = null;
            T ret = head.val;
            ChainNode<T> k = head.next;
            head.next = null;
            if (k != null) k.pre = null;
            //DestoryNode(head);
            ChainNode<T>.Destroy(head);
            head = k;
            count--;
            return ret;
        }

        public T PopBack()
        {
            if (tail == null) return default(T);
            T ret = tail.val;
            ChainNode<T> k = tail.pre;
            if (k != null)
            {
                k.next = null;
                ChainNode<T>.Destroy(tail);
                tail = k;
            }
            else
            {
                ChainNode<T>.Destroy(tail);
                head = tail = null;
            }
            count--;
            return ret;
        }

        public void DoChain(Action<T> action)
        {
            ChainNode<T> k = head;
            while (k != null)
            {
                action(k.val);
                k = k.next;
            }
        }

        public bool Contains(T v)
        {
            ChainNode<T> k = head;
            while (k != null)
            {
                if(k.val.Equals(v))
                {
                    iterator = k;
                    return true;
                }
                k = k.next;
            }
            return false;
        }

        public T FirstMatch(Predicate<T> predFn)
        {
            return FirstMatch<T>(predFn, t => t);
        }

        public ReturnType FirstMatch<ReturnType>(Predicate<T> predFn, Converter<T, ReturnType> convertFunc)
        {
            ChainNode<T> k = head;
            while (k != null)
            {
                iterator = k;
                if (predFn(Current))
                {
                    return convertFunc(Current);
                }
                k = k.next;
            }
            return default(ReturnType);
        }
    }

}

