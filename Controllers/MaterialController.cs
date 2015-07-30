using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBMSW.Models;
using Newtonsoft.Json;
using System.Data;

namespace EBMSW.Controllers
{
    public class MaterialController : Controller
    {
        IMaterial IM = new MaterialBase();


        public PartialViewResult CreateMaterial(Guid ID)
        {
            ViewData["LinkMainCID"] = ID;
            IBrand IB = new BrandBase();
            List<Brand> BL = IB.GetBrandList(string.Empty, ID);
            return PartialView("CreateMaterial", BL);
        }

        [HttpPost]
        public RedirectToRouteResult CreateMaterialPost(Guid ID, FormCollection FC)
        {
            Guid LinkMainCID = ID;
            Material M = new Material();
            TryUpdateModel<Material>(M, FC);
            try
            {
                Guid MatID = IM.CreateBaseMaterial(M, string.Empty, LinkMainCID);

                //产品创建成功，创建图片文件
                try
                {
                    HttpPostedFileBase MatImg = Request.Files["ImagePath"];
                    MyAlbumImageUpload MyMatImg = new MyAlbumImageUpload();
                    string[] ImgPath = MyMatImg.CreateMatImgProcess(MatImg, "MaterialImg/" + LinkMainCID.ToString());
                    IM.UpdateMaterialImg(MatID, ImgPath[0], ImgPath[1], ImgPath[2]);
                }
                catch
                {
                    //Dothing
                }

                try
                {
                    HttpPostedFileBase MatFile = Request.Files["FilePath"];
                    MyNormalUploadFile MyUpload = new MyNormalUploadFile();
                    string FilePath = MyUpload.NormalUpLoadFileProcess(MatFile, "MaterialFile/" + LinkMainCID.ToString());
                    IM.UpdateMatFilePath(MatID, FilePath);
                }
                catch
                {

                }
                return RedirectToAction("UpdateMaterialBase", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                TempData["MatName"] = M.MatName;
                TempData["MatSpecifications"] = M.MatSpecifications;
                TempData["MatBrand"] = M.MatBrand;
                TempData["MatManufacturerSn"] = M.MatManufacturerSn;
                TempData["MatUnit"] = M.MatUnit;
                TempData["MatSummary"] = M.MatSummary;
                return RedirectToAction("CreateMaterial", "Material", new { ID = ID });
            }
        }


        public PartialViewResult UpdateMaterialBase(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            IBrand IB = new BrandBase();
            List<Brand> BL = IB.GetBrandList(string.Empty, M.LinkMainCID);
            ViewData["BL"] = BL;
            return PartialView("UpdateMaterialBase", M);
        }
         
        public PartialViewResult MaterialImgAndFileUpload(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            return PartialView("MaterialImgAndFileUpload", M);
        }


        [HttpPost]
        public RedirectToRouteResult UpdateMaterialBasePost(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;

            try
            {
                Material M = new Material();
                TryUpdateModel<Material>(M, FC);
                M.MatID = ID;
                IM.UpdateBaseMaterial(M, string.Empty);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateMaterialBase", "Material", new { ID = ID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateMaterialBase", "Material", new { ID = ID });
            }


        }



        [HttpPost]
        public string UpdateMaterialPriceAndLeadTime(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            Guid MatID = ID;

            try
            {
                decimal CostPrice = 0;
                decimal MarketPrice = 0;
                int LeadTime = 0;

                try
                {
                    CostPrice = Convert.ToDecimal(FC["CostPrice"]);
                }
                catch
                {
                    CostPrice = 0;
                }

                try
                {
                    MarketPrice = Convert.ToDecimal(FC["MarketPrice"]);
                }
                catch
                {
                    MarketPrice = 0;
                }

                try
                {
                    LeadTime = Convert.ToInt32(FC["LeadTime"]);
                }
                catch
                {
                    LeadTime = 0;
                }

                IM.UpdateMaterialPriceAndLeadTime(MatID, CostPrice, MarketPrice, LeadTime);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }
            return result;
        }

        [HttpPost]
        public string UpdateStoreInfo(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            Guid MatID = ID;

            try
            {
                int SafetyStock = 0;
                int WeeklySales = 0;

                try
                {
                    SafetyStock = Convert.ToInt16(FC["SafetyStock"]);
                }
                catch
                {
                    SafetyStock = 0;
                }

                try
                {
                    WeeklySales = Convert.ToInt32(FC["WeeklySales"]);
                }
                catch
                {
                    WeeklySales = 0;
                }
                IM.UpdateStoreInfo(MatID, SafetyStock, WeeklySales);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }
            return result;
        }

        [HttpPost]
        public string UpdateMaterialCatID(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                Guid CatID = new Guid(FC["NewToCatID"]);
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatIDListStr"]);
                IM.UpdateMaterialCatID(MatList.Where(x=>x != Guid.Empty).ToList(), CatID, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsSpotGood(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsSpotGood(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNotSpotGood(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNotSpotGood(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }


        [HttpPost]
        public string MatIsPublic(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsPublic(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNotPublic(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNotPublic(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsTop(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsTop(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNew(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNew(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNotTop(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNotTop(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNotNew(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNotNew(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsPromotion(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                int IsPromotionRate = Convert.ToInt32(FC["IsPromotionRate"]);
                DateTime IsPromotionEndDate = Convert.ToDateTime(FC["IsPromotionEndDate"]);
                IM.MatIsPromotion(MatList, IsPromotionEndDate, IsPromotionRate, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }

        [HttpPost]
        public string MatIsNotPromotion(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            try
            {
                Guid LinkMainCID = ID;
                List<Guid> MatList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.MatIsNotPromotion(MatList, LinkMainCID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }

            return result;
        }


        [HttpPost]
        public RedirectToRouteResult DeleteMaterialItem(Guid ID)
        {
            Guid MatID = ID;

            try
            {
                IM.DeleteMaterialItem(MatID);
                TempData["Message"] = "删除成功！";
                return RedirectToAction("SysMessage", "Material");
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateMaterialBase", "Material", new { ID = ID });
            }
        }


        [HttpPost]
        public RedirectToRouteResult MaterialImgUploadPost(Guid ID, Guid LinkMainCID)
        {
            try
            {
                HttpPostedFileBase MatImg = Request.Files["ImagePath"];
                MyAlbumImageUpload MyMatImg = new MyAlbumImageUpload();
                string[] ImgPath = MyMatImg.CreateMatImgProcess(MatImg, "MaterialImg/" + LinkMainCID.ToString());
                IM.UpdateMaterialImg(ID, ImgPath[0], ImgPath[1], ImgPath[2]);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }
            return RedirectToAction("MaterialImgAndFileUpload", "Material", new { ID = ID });
        }

        [HttpPost]
        public RedirectToRouteResult MaterialFileUpLoadPost(Guid ID, Guid LinkMainCID)
        {
            try
            {
                HttpPostedFileBase MatFile = Request.Files["FilePath"];
                MyNormalUploadFile MyUpload = new MyNormalUploadFile();
                string FilePath = MyUpload.NormalUpLoadFileProcess(MatFile, "MaterialFile/" + LinkMainCID.ToString());
                IM.UpdateMatFilePath(ID, FilePath);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }

            return RedirectToAction("MaterialImgAndFileUpload", "Material", new { ID = ID });
        }

        [HttpPost]
        public RedirectToRouteResult MatImgAndFileDelete(Guid ID)
        {
            string FileType = Request.Form["FileType"];
            switch (FileType)
            {
                case "Img":
                    IM.DeleteMatImg(ID);
                    break;

                case "File":
                    IM.DeleteMatFile(ID);
                    break;
                default:
                    IM.DeleteMatImg(ID);
                    IM.DeleteMatFile(ID);
                    break;
            }
            return RedirectToAction("MaterialImgAndFileUpload", "Material", new { ID = ID });
        }

        public PartialViewResult UpdateMaterialShowGroup(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            List<Material> ML = IM.GetMaterialByGroupMatID(Guid.Empty, M.MatID);
            ViewData["ML"] = ML;
            return PartialView("UpdateMaterialShowGroup", M);
        }


        public PartialViewResult UpdateMaterialDetail(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            return PartialView("UpdateMaterialDetail", M);
        }

        [HttpPost]
        [ValidateInput(false)]
        public RedirectToRouteResult UpdateMaterialDetailPost(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;

            try
            {
                string MoreDetail = FC["MoreDetail"];
                IM.UpdateMaterilMoreDetail(MatID, MoreDetail);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateMaterialDetail", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateMaterialDetail", "Material", new { ID = MatID });
            }
        }

        [HttpGet]
        public string FindMaterialByPage(Guid ID)
        {
            IM.AutoCallBackPromotion();

            Guid LinkMainCID = ID;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;
            string MatBrand = string.Empty;
            string CatName = string.Empty;
            string IsImg = string.Empty;
            Guid MatBrandID = Guid.Empty;
            string IsStore = string.Empty;
            string IsStatUs = string.Empty;


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

            try
            {
                MatBrand = Request.QueryString["MatBrand"].Trim();
            }
            catch
            {
                MatBrand = string.Empty;
            }

            try
            {
                CatName = Request.QueryString["CatName"].Trim();
            }
            catch
            {
                CatName = string.Empty;
            }

            try
            {
                IsImg = Request.QueryString["IsImg"].Trim();
            }
            catch
            {
                IsImg = string.Empty;
            }

            try
            {
                IsStore = Request.QueryString["IsStore"].Trim();
            }
            catch
            {
                IsStore = string.Empty;
            }

            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }
            
            try
            {
                IsStatUs = Request.QueryString["IsStatUs"].Trim();
            }
            catch
            {
                IsStatUs = string.Empty;
            }
            

            int TotalRecord = 0;
            List<Material> ML = IM.FindMaterialByPage(PageIndex, PageSize, out TotalRecord, Keyword, MatBrand, CatName, IsImg, IsStore, MatBrandID, IsStatUs, LinkMainCID).ToList();
            MaterialPageList MPageList = new MaterialPageList();
            MPageList.TotalRecord = TotalRecord;
            MPageList.PageIndex = PageIndex;
            MPageList.PageSize = PageSize;
            MPageList.Rows = ML;
            return JsonConvert.SerializeObject(MPageList);

        }

        [HttpGet]
        public string FindMaterialIsSpotGoodByPage(Guid ID)
        {

            Guid LinkMainCID = ID;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;
            Guid MatBrandID = Guid.Empty;
            int IsSpotGood = 0;


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


            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }      

            try
            {
                IsSpotGood = Convert.ToInt32(Request.QueryString["IsSpotGood"].Trim());
            }
            catch
            {
                IsSpotGood = 0;
            }


            int TotalRecord = 0;
            List<Material> ML = IM.FindMaterialIsSpotGoodByPage(PageIndex, PageSize, out TotalRecord, Keyword, MatBrandID, IsSpotGood, LinkMainCID).ToList();
            MaterialPageList MPageList = new MaterialPageList();
            MPageList.TotalRecord = TotalRecord;
            MPageList.PageIndex = PageIndex;
            MPageList.PageSize = PageSize;
            MPageList.Rows = ML;
            return JsonConvert.SerializeObject(MPageList);

        }


        [HttpGet]
        public string FindMaterialPageBySearchList(Guid ID)
        {
            IM.AutoCallBackPromotion();

            Guid LinkMainCID = ID;
            Guid CID = Guid.Empty;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;
            string MatBrand = string.Empty;
            Guid CatID = Guid.Empty;
            Guid MatBrandID = Guid.Empty;
            string IsStore = string.Empty;
            string IsStatUs = string.Empty;

            try
            {
                CID = new Guid(Request.QueryString["CID"].ToString());
            }
            catch
            {
                CID = Guid.Empty;
            }
            
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

            try
            {
                MatBrand = Request.QueryString["MatBrand"].Trim();
            }
            catch
            {
                MatBrand = string.Empty;
            }

            try
            {
                CatID = new Guid(Request.QueryString["CatID"].Trim());
            }
            catch
            {
                CatID = Guid.Empty;
            }

         

            try
            {
                IsStore = Request.QueryString["IsStore"].Trim();
            }
            catch
            {
                IsStore = string.Empty;
            }

            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }

            try
            {
                IsStatUs = Request.QueryString["IsStatUs"].Trim();
            }
            catch
            {
                IsStatUs = string.Empty;
            }


            MaterialPageBySearchList MPageList = IM.FindMaterialPageBySearchList(PageIndex, PageSize, Keyword, MatBrand, MatBrandID, CatID, IsStatUs, CID, LinkMainCID);
            return JsonConvert.SerializeObject(MPageList);

        }


        [HttpGet]
        public string FindMaterialPageBySearchListByQDT()
        {
            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;
            string MatBrand = string.Empty;
            Guid CatID = Guid.Empty;
            string IsStatUs = string.Empty;

           
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

            try
            {
                MatBrand = Request.QueryString["MatBrand"].Trim();
            }
            catch
            {
                MatBrand = string.Empty;
            }

            try
            {
                CatID = new Guid(Request.QueryString["CatID"].Trim());
            }
            catch
            {
                CatID = Guid.Empty;
            }

            try
            {
                IsStatUs = Request.QueryString["IsStatUs"].Trim();
            }
            catch
            {
                IsStatUs = string.Empty;
            }


            MaterialPageBySearchList MPageList = IM.FindMaterialPageBySearchListByQDT(PageIndex, PageSize, Keyword, MatBrand, CatID, IsStatUs);
            return JsonConvert.SerializeObject(MPageList);

        }


        [HttpGet]
        public string FindMaterialPageByIsGroupSearchList(Guid ID)
        {
            Guid LinkMainCID = ID;

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

            MaterialPageBySearchList MPageList = IM.FindMaterialPageByIsGroupSearchList(PageIndex, PageSize, Keyword, LinkMainCID);
            return JsonConvert.SerializeObject(MPageList);

        }


        [HttpGet]
        public string FindMaterialForCustomerByPage(Guid ID)
        {
            Guid LinkMainCID = ID;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 30;

            string Keyword = string.Empty;
            string MatBrand = string.Empty;
            string CatName = string.Empty;
            string IsStatUs = string.Empty;
            Guid MatBrandID = Guid.Empty;
            Guid CustomerID = Guid.Empty;
            int TotalRecord;


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

            try
            {
                MatBrand = Request.QueryString["MatBrand"].Trim();
            }
            catch
            {
                MatBrand = string.Empty;
            }

            try
            {
                CatName = Request.QueryString["CatName"].Trim();
            }
            catch
            {
                CatName = string.Empty;
            }

            try
            {
                IsStatUs = Request.QueryString["IsStatUs"].Trim();
            }
            catch
            {
                IsStatUs = string.Empty;
            }
            
           

            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }

            try
            {
                CustomerID = new Guid(Request.QueryString["CustomerID"].Trim());
            }
            catch
            {
                CustomerID = Guid.Empty;
            }

            List<Material> ML = IM.FindMaterialForCustomerByPage(PageIndex, PageSize, out TotalRecord, Keyword, MatBrand, CatName, MatBrandID, IsStatUs, CustomerID, LinkMainCID);

            MaterialPageList MPageList = new MaterialPageList();
            MPageList.PageIndex = PageIndex;
            MPageList.PageSize = PageSize;
            MPageList.Rows = ML;
            MPageList.TotalRecord = TotalRecord;
            return JsonConvert.SerializeObject(MPageList);
        }


        [HttpGet]
        public string GetMaterialItemDB(Guid ID)
        {
            Guid MatID = ID;
            return JsonConvert.SerializeObject(IM.GetMaterialItemDB(MatID));
        }

        [HttpGet]
        public string GetMaterialItem(Guid ID)
        {
            Guid MatID = ID;
            Guid CID = Guid.Empty;
            try {
                CID = new Guid(Request.QueryString["CID"].ToString());
            }
            catch {
                CID = Guid.Empty;
            }
            return JsonConvert.SerializeObject(IM.GetMaterialItem(MatID, CID));
        }

        [HttpGet]
        public string GetMaterialByGroupMatID(Guid ID)
        {
            Guid MatID = ID;

            Guid CID = Guid.Empty;
            try {
                CID = new Guid(Request.QueryString["CID"].ToString());
            }
            catch {
                CID = Guid.Empty;
            }
            return JsonConvert.SerializeObject(IM.GetMaterialByGroupMatID(CID, MatID));
        }


        public PartialViewResult CreateMaterialByExcel(Guid ID)
        {
            Guid LinkMainCID = ID;
            ViewData["LinkMainCID"] = LinkMainCID;
            List<MatExcel> ML = IM.GetMatExcelList(LinkMainCID);
            IBrand IB = new BrandBase();
            List<Brand> BL = IB.GetBrandList(string.Empty, LinkMainCID);
            ViewData["BL"] = BL;
            return PartialView("CreateMaterialByExcel", ML);
        }


        [HttpPost]
        public RedirectToRouteResult MatExcelDBToMaterial(Guid ID)
        {
            Guid LinkMainCID = ID;
            try
            {
                Guid MatBrandID = new Guid(Request.Form["P_BrandID"]);
                string DefBrandName = Request.Form["P_DefBrandName"];
                string DefBrandType = Request.Form["P_DefBrandType"];
                IM.MatExcelDBToMaterial(LinkMainCID, MatBrandID, DefBrandName, DefBrandType);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }
            return RedirectToAction("CreateMaterialByExcel", "Material", new { ID = LinkMainCID });
        }

        [HttpPost]
        public RedirectToRouteResult CreateMaterialByExcelPost(Guid ID)
        {
            Guid LinkMainCID = ID;
            try
            {
                HttpPostedFileBase File = Request.Files["ExcelFile"];
                IM.InputMaterialByExcel(File, LinkMainCID);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }
            return RedirectToAction("CreateMaterialByExcel", "Material", new { ID = LinkMainCID });
        }

        [HttpPost]
        public RedirectToRouteResult DeleteMatExcelDBItem(Guid ID)
        {
            Guid LinkMainCID = ID;
            try
            {
                IM.DeleteMatExcelDBItem(LinkMainCID);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }
            return RedirectToAction("CreateMaterialByExcel", "Material", new { ID = LinkMainCID });
        }

        [HttpGet]
        public ActionResult MatListToExcel(Guid ID)
        {
            Guid LinkMainCID = ID;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 4000;

            string Keyword = string.Empty;
            Guid MatBrandID = Guid.Empty;

            try
            {
                Keyword = Request.QueryString["Keyword"].Trim();
            }
            catch
            {
                Keyword = string.Empty;
            }

            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }

            int TotalRecord = 0;
            List<Material> ML = IM.FindMaterialByPage(PageIndex, PageSize, out TotalRecord, Keyword, string.Empty, string.Empty, string.Empty, string.Empty, MatBrandID, string.Empty, LinkMainCID).Where(x=>x.IsGroupMat <= 0).ToList();

            string Path = string.Empty;

            //设定表头
            DataTable dt = new DataTable("TempInvoice");
            //设定dataTable表头
            DataColumn myDataColumn = new DataColumn();
            string[] TableHeads = {
                        "ID",
                        "图",
                        "产品编号",
                        "产品名称",
                        "规格型号",
                        "品牌",
                        "制造商编号",
                        "单位",
                        "未税采购价",
                        "未税厂商面价",
                        "货期(天)",
                        "供应商",
              };

            foreach (string TableHead in TableHeads)
            {
                //TableHead
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.String");
                myDataColumn.ColumnName = TableHead;
                myDataColumn.ReadOnly = true;
                myDataColumn.Unique = false;  //获取或设置一个值，该值指示列的每一行中的值是否必须是唯一的。
                dt.Columns.Add(myDataColumn);
            }

            try
            {
                DataRow newRow;
                foreach (var x in ML)
                {
                    newRow = dt.NewRow();

                    newRow["ID"] = x.MatID;
                    if(!string.IsNullOrEmpty(x.MatImgPath))
                    {
                        newRow["图"] = "有";
                    }
                    else
                    {
                        newRow["图"] = string.Empty;
                    }
                    newRow["产品编号"] = x.MatSn;
                    newRow["产品名称"] = x.MatName;
                    newRow["规格型号"] = x.MatSpecifications;
                    newRow["品牌"] = x.MatBrand;
                    newRow["制造商编号"] = x.MatManufacturerSn;
                    newRow["单位"] = x.MatUnit;
                    newRow["未税采购价"] = x.CostPrice;
                    newRow["未税厂商面价"] = x.MarketPrice;
                    newRow["货期(天)"] = x.LeadTime;
                    newRow["供应商"] = x.SupplierName;
                    dt.Rows.Add(newRow);
                }
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }
            catch
            {
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }

            return File(Path, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "产品价格表" + DateTime.Now.ToString("yyyy-MM-dd"));

        }

        [HttpPost]
        public ActionResult DownBrandTempExcel(FormCollection FC)
        {
            //获取Brand
            IBrand IB = new BrandBase();
            Guid BID = new Guid(FC["D_BrandID"]);
            Brand B = IB.GetBrandItem(BID);

            string Path = string.Empty;

            //设定表头
            DataTable dt = new DataTable("TempInvoice");
            //设定dataTable表头
            DataColumn myDataColumn = new DataColumn();
            string[] TableHeads = {
                        "产品编号",
                        "产品名称",
                        "规格型号",
                        "制造商编号",
                        "品牌",
                        "品牌分类",
                        "单位",
                        "未税采购价",
                        "未税厂商面价",
                        "货期(天)",
                        "产品简述",
              };

            foreach (string TableHead in TableHeads)
            {
                //TableHead
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.String");
                myDataColumn.ColumnName = TableHead;
                myDataColumn.ReadOnly = true;
                myDataColumn.Unique = false;  //获取或设置一个值，该值指示列的每一行中的值是否必须是唯一的。
                dt.Columns.Add(myDataColumn);
            }

            try
            {
                DataRow newRow;
                for (int i = 0; i <= 20; i++ )
                {
                    newRow = dt.NewRow();

                    newRow["产品编号"] = string.Empty;
                    newRow["产品名称"] = string.Empty;
                    newRow["规格型号"] = string.Empty;
                    newRow["制造商编号"] = string.Empty;
                    newRow["品牌"] = B.BrandName;
                    newRow["品牌分类"] = B.BrandType;
                    newRow["单位"] = string.Empty;
                    newRow["未税采购价"] = string.Empty;
                    newRow["未税厂商面价"] = string.Empty;
                    newRow["货期(天)"] = string.Empty;
                    newRow["产品简述"] = string.Empty;
                    dt.Rows.Add(newRow);
                }
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }
            catch
            {
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }


            string FileNameStr = string.Empty;
            if(!string.IsNullOrEmpty(B.BrandType))
            {
                FileNameStr = B.BrandName + "-" + B.BrandType;
            }
            else
            {
                FileNameStr = B.BrandName;
            }
            return File(Path, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileNameStr+"-数据模板");

        }

        [HttpGet]
        public ActionResult MatListStoreToExcel(Guid ID)
        {
            Guid LinkMainCID = ID;

            //定义查询参数
            int PageIndex = 1;
            int PageSize = 4000;

            string Keyword = string.Empty;
            Guid MatBrandID = Guid.Empty;

            try
            {
                Keyword = Request.QueryString["Keyword"].Trim();
            }
            catch
            {
                Keyword = string.Empty;
            }

            try
            {
                MatBrandID = new Guid(Request.QueryString["MatBrandID"].Trim());
            }
            catch
            {
                MatBrandID = Guid.Empty;
            }

            int TotalRecord = 0;
            List<Material> ML = new List<Material>();

            if (MatBrandID == Guid.Empty && string.IsNullOrEmpty(Keyword))
            {
                ML = IM.FindMaterialByPage(PageIndex, PageSize, out TotalRecord, Keyword, string.Empty, string.Empty, string.Empty, "IsStore", MatBrandID, string.Empty, LinkMainCID).Where(x => x.IsGroupMat <= 0).ToList();
            }
            else
            {
                ML = IM.FindMaterialByPage(PageIndex, PageSize, out TotalRecord, Keyword, string.Empty, string.Empty, string.Empty, string.Empty, MatBrandID, string.Empty, LinkMainCID).Where(x => x.IsGroupMat <= 0).ToList();
            }


            string Path = string.Empty;

            //设定表头
            DataTable dt = new DataTable("TempInvoice");
            //设定dataTable表头
            DataColumn myDataColumn = new DataColumn();
            string[] TableHeads = {
                        "ID",
                        "产品编号",
                        "产品名称",
                        "规格型号",
                        "品牌",
                        "制造商编号",
                        "单位",
                        "未税采购价",
                        "未税面价",
                        "货期(天)",
                        "每周销量",
                        "安全库存",
                        "再订购量",
              };

            foreach (string TableHead in TableHeads)
            {
                //TableHead
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.String");
                myDataColumn.ColumnName = TableHead;
                myDataColumn.ReadOnly = true;
                myDataColumn.Unique = false;  //获取或设置一个值，该值指示列的每一行中的值是否必须是唯一的。
                dt.Columns.Add(myDataColumn);
            }

            try
            {
                DataRow newRow;
                foreach (var x in ML)
                {
                    newRow = dt.NewRow();

                    newRow["ID"] = x.MatID;
                    newRow["产品编号"] = x.MatSn;
                    newRow["产品名称"] = x.MatName;
                    newRow["规格型号"] = x.MatSpecifications;
                    newRow["品牌"] = x.MatBrand;
                    newRow["制造商编号"] = x.MatManufacturerSn;
                    newRow["单位"] = x.MatUnit;
                    newRow["未税采购价"] = x.CostPrice;
                    newRow["未税厂商面价"] = x.MarketPrice;
                    newRow["货期(天)"] = x.LeadTime;
                    newRow["每周销量"] = x.WeeklySales;
                    newRow["安全库存"] = x.SafetyStock;
                    newRow["再订购量"] = x.SingleOrderNumber;
                    dt.Rows.Add(newRow);
                }
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }
            catch
            {
                Path = MyExcel.CreateNewExcel(dt, string.Empty);
            }

            return File(Path, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "产品库存设置" + DateTime.Now.ToString("yyyy-MM-dd"));

        }

        public PartialViewResult SearchPurPriceInputExcel(Guid ID)
        {
            Guid LinkMainCID = ID;
            ViewData["LinkMainCID"] = LinkMainCID;
            List<MatExcel> ML = new List<MatExcel>();
            string MLJson = string.Empty;
            try
            {
                if (Request.Files["ExcelFile"] != null)
                {
                    HttpPostedFileBase File = Request.Files["ExcelFile"];
                    ML = IM.InputMaterialByPurPriceAndLeadTimeExcel(File, LinkMainCID);
                    MLJson = JsonConvert.SerializeObject(ML);
                }

            }
            catch (Exception Ex)
            {
                ML = new List<MatExcel>();
                TempData["ML"] = null;
                TempData["Error"] = Ex.Message.ToString();
                MLJson = string.Empty;
            }

            ViewData["MLJson"] = MLJson;
            return PartialView("SearchPurPriceInputExcel", ML);
        }

        [HttpPost]
        public RedirectToRouteResult SearchPurPriceInputExcelPost(Guid ID, FormCollection FC)
        {
            Guid LinkMainCID = ID;
            try
            {
                string MLJson = FC["MLJson"];
                List<MatExcel> ML = JsonConvert.DeserializeObject<List<MatExcel>>(MLJson);
                IM.UpdateMaterialPriceAndLeadTimeBatch(LinkMainCID, ML);
                TempData["BatchOK"] = "共" + ML.Count() + "个产品，价格及交期批量更新成功！";
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
            }
            return RedirectToAction("SearchPurPriceInputExcel", "Material", new { ID = LinkMainCID });
        }

        #region MatGroup
        public PartialViewResult CreateGroupMat(Guid ID)
        {
            Guid LinkMainCID = ID;
            ViewData["LinkMainCID"] = LinkMainCID;
            IBrand IB = new BrandBase();
            List<Brand> BL = IB.GetBrandList(string.Empty, ID);
            return PartialView("CreateGroupMat", BL);
        }

        [HttpPost]
        public RedirectToRouteResult CreateGroupMatPost(Guid ID, FormCollection FC)
        {
            Guid LinkMainCID = ID;
            Material M = new Material();
            TryUpdateModel<Material>(M, FC);
            try
            {
                Guid MatID = IM.CreateBaseGroupMaterial(M, string.Empty, LinkMainCID);

                //产品创建成功，创建图片文件
                try
                {
                    HttpPostedFileBase MatImg = Request.Files["ImagePath"];
                    MyAlbumImageUpload MyMatImg = new MyAlbumImageUpload();
                    string[] ImgPath = MyMatImg.CreateMatImgProcess(MatImg, "MaterialImg/" + LinkMainCID.ToString());
                    IM.UpdateMaterialImg(MatID, ImgPath[0], ImgPath[1], ImgPath[2]);
                }
                catch
                {
                    //Dothing
                }

                try
                {
                    HttpPostedFileBase MatFile = Request.Files["FilePath"];
                    MyNormalUploadFile MyUpload = new MyNormalUploadFile();
                    string FilePath = MyUpload.NormalUpLoadFileProcess(MatFile, "MaterialFile/" + LinkMainCID.ToString());
                    IM.UpdateMatFilePath(MatID, FilePath);
                }
                catch
                {

                }
                return RedirectToAction("UpdateGroupMaterialBase", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                TempData["MatName"] = M.MatName;
                TempData["MatSpecifications"] = M.MatSpecifications;
                TempData["MatBrand"] = M.MatBrand;
                TempData["MatManufacturerSn"] = M.MatManufacturerSn;
                TempData["MatUnit"] = M.MatUnit;
                TempData["MatSummary"] = M.MatSummary;
                return RedirectToAction("CreateGroupMatPost", "Material", new { ID = ID });
            }
        }

        public PartialViewResult UpdateGroupMaterialBase(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            IBrand IB = new BrandBase();
            List<Brand> BL = IB.GetBrandList(string.Empty, M.LinkMainCID);
            ViewData["BL"] = BL;
            return PartialView("UpdateGroupMaterialBase", M);
        }

        [HttpPost]
        public RedirectToRouteResult UpdateGroupMaterialBasePost(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;

            try
            {
                Material M = new Material();
                TryUpdateModel<Material>(M, FC);
                M.MatID = ID;
                IM.UpdateBaseMaterial(M, string.Empty);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateGroupMaterialBase", "Material", new { ID = ID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateGroupMaterialBase", "Material", new { ID = ID });
            }


        }

        public PartialViewResult UpdateGroupMaterialDetail(Guid ID)
        {
            Material M = IM.GetMaterialItemDB(ID);
            return PartialView("UpdateGroupMaterialDetail", M);
        }

        [HttpPost]
        [ValidateInput(false)]
        public RedirectToRouteResult UpdateGroupMaterialDetailPost(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;

            try
            {
                string MoreDetail = FC["MoreDetail"];
                IM.UpdateMaterilMoreDetail(MatID, MoreDetail);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateGroupMaterialDetail", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateGroupMaterialDetail", "Material", new { ID = MatID });
            }
        }

        public PartialViewResult UpdateGroupMaterialList(Guid ID)
        {
            Guid MatID = ID;
            List<Material> ML = IM.GetMaterialByGroupMatID(Guid.Empty, MatID);
            ViewData["MatID"] = MatID;
            return PartialView("UpdateGroupMaterialList", ML);
        }

        public PartialViewResult UpdateGroupMaterialSearchList(Guid ID)
        {
            Guid MatID = ID;
            Material MT = IM.GetMaterialItemDB(MatID);
            List<Material> IsGroupML = IM.GetMaterialByGroupMatID(Guid.Empty, MatID);

            string Keyword = string.Empty;
            string MatBrand = string.Empty;

            try
            {
                Keyword = Request.QueryString["Keyword"].ToString().Trim();
            }
            catch
            {
                Keyword = MT.MatName;
            }


            try
            {
                MatBrand = Request.QueryString["MatBrand"].ToString().Trim();
            }
            catch
            {
                MatBrand = MT.MatBrand;
            }

            int TotalRecord;
            List<Material> ML = IM.FindMaterialByPage(1, 400, out TotalRecord, Keyword, MatBrand, string.Empty, string.Empty, string.Empty, Guid.Empty, string.Empty, MT.LinkMainCID).ToList();
            //进行过滤自身及已匹配项

            ViewData["MatID"] = MatID;
            ViewData["Keyword"] = Keyword;
            ViewData["MatBrand"] = MatBrand;
            ViewData["IsGroupML"] = IsGroupML;

            return PartialView("UpdateGroupMaterialSearchList", ML);
        }


        [HttpPost]
        public string LinkGroupMatList(Guid ID, FormCollection FC)
        {
            string result = string.Empty;
            Guid MatID = ID;
            try
            {
                List<Guid> MatIDList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.LinkGroupMatList(MatIDList, MatID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }
            return result;
        }

        [HttpPost]
        public RedirectToRouteResult NotLinkGroupMatList(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;
            try
            {
                List<Guid> MatIDList = CommonFunctionLib.GuidListStrToGuidArray(FC["MatID"]);
                IM.NotLinkGroupMatList(MatIDList);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateGroupMaterialList", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateGroupMaterialList", "Material", new { ID = MatID });
            }
        }

        [HttpPost]
        public RedirectToRouteResult GroupMatToCloneList(Guid ID, FormCollection FC)
        {
            Guid MatID = ID;
            try
            {
                IM.LinkGroupMatClone(MatID);
                TempData["Success"] = "Success";
                return RedirectToAction("UpdateGroupMaterialList", "Material", new { ID = MatID });
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Ex.Message.ToString();
                return RedirectToAction("UpdateGroupMaterialList", "Material", new { ID = MatID });
            }
        }

        #endregion
        
        [HttpGet]
        public string IsToMatketDB(Guid ID)
        {
            string result = string.Empty;
            Guid MatID = ID;
            try
            {
                Guid CID = new Guid(Request.QueryString["CID"].ToString());
                ICustMat ICMat = new CustMatBase();
                ICMat.IsToMatketDB(MatID, CID);
            }
            catch (Exception Ex)
            {
                result = Ex.Message.ToString();
            }
            return result;
        }


        public PartialViewResult SysMessage()
        {
            return PartialView("SysMessage");
        }


	}
}