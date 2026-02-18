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
        /// 每次增加物资都需要调用这个
        /// </summary>
        /// <param name="id"></param>
        public void AddBook(int id)
        {
            //if (id == 1200001) { return; }
            //if (bookList.Contains(id) || spBookList.Contains(id)) { return; }
            //int len = makeAry.Length;
            //for (int i = 0; i < len; i++)
            //{
            //    MakeCA ca = makeAry[i];
            //    if (FindItem(ca, id) == true)
            //    {
            //        if (bookList.Contains(ca.id) == false)
            //        {
            //            tipManager.InitTip("新物资获得，解锁了相关家具" + ca.name);
            //            bookList.Add(ca.id);
            //        }
            //    }
            //}
        }
     
        public void RefreshAsset()
        {

        }
    }
}