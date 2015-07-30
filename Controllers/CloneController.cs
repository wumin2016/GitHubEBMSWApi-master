using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMSW.Models;
using Newtonsoft.Json;

namespace EBMSW.Controllers
{
    public class CloneController : Controller
    {
        IClone IC = new CloneBase();

        public string CloneMaterialToNewMainCom(FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid SourceMainCID = new Guid(FC["SourceMainCID"]);
                Guid ToMainCID = new Guid(FC["ToMainCID"]);
                List<Guid> SourceBrandID = CommonFunctionLib.GuidListStrToGuidArray(FC["BID"]);
                result = IC.CloneMaterialToNewMainCom(SourceMainCID, SourceBrandID, ToMainCID).ToString();
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }
            return result;
        }


        public string GetBrandByMainComList(FormCollection FC)
        {

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;

            try
            {
                PageIndex = Convert.ToInt32(Request.QueryString["PageIndex"]);
                if (PageIndex <= 0)
                {
                    PageIndex = 1;
                }
            }
            catch
            {
                PageIndex = 1;
            }

            try
            {
                PageSize = Convert.ToInt32(Request.QueryString["PageSize"]);
                if (PageSize <= 0)
                {
                    PageSize = 30;
                }
            }
            catch
            {
                PageSize = 30;
            }

            try
            {
                Keyword = Request.QueryString["Keyword"].Trim();
            }
            catch
            {
                Keyword = string.Empty;
            }


            int TotalRecord = 0;
            List<BrandByMainCom> ML = IC.GetBrandByMainComList(PageIndex, PageSize, out TotalRecord, Keyword).ToList();
            BrandByMainComPageList MPageList = new BrandByMainComPageList();
            MPageList.TotalRecord = TotalRecord;
            MPageList.PageIndex = PageIndex;
            MPageList.PageSize = PageSize;
            MPageList.Rows = ML;
            return JsonConvert.SerializeObject(MPageList);
        }

    }
}