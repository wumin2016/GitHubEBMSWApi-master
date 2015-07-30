using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EBMSW.Models
{
    public interface IMaterial
    {
        List<Material> FindMaterialByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, string MatBrand, string CatName, string IsImg, string IsStore, Guid MatBrandID, string IsStatUs, Guid LinkMainCID);
        List<Material> FindMaterialIsSpotGoodByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, Guid MatBrandID, int IsSpotGood, Guid LinkMainCID);
        List<Material> FindMaterialForCustomerByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, string MatBrand, string CatName, Guid MatBrandID, string IsStatUs, Guid CustomerID, Guid LinkMainCID);
        List<Material> GetMaterialByGroupMatID(Guid CID, Guid MatID);

        MaterialPageBySearchList FindMaterialPageBySearchListByQDT(int PageIndex, int PageSize, string Keyword, string MatBrand, Guid CatID, string IsStatUs);
        MaterialPageBySearchList FindMaterialPageBySearchList(int PageIndex, int PageSize, string Keyword, string MatBrand, Guid MatBrandID, Guid CatID, string IsStatUs, Guid CID, Guid LinkMainCID);
        MaterialPageBySearchList FindMaterialPageByIsGroupSearchList(int PageIndex, int PageSize, string Keyword, Guid LinkMainCID);        
        Guid CreateBaseMaterial(Material M, string UserFullName, Guid LinkMainCID);
        Guid CreateBaseGroupMaterial(Material M, string UserFullName, Guid LinkMainCID);
        void UpdateBaseMaterial(Material M, string UserFullName);
        void UpdateMaterialPriceAndLeadTime(Guid MatID, decimal CostPrice, decimal MarketPrice, int LeadTime);
        void UpdateMaterialPriceAndLeadTimeBatch(Guid LinkMainCID, List<MatExcel> ML);
        void UpdateStoreInfo(Guid MatID, int SafetyStock, int WeeklySales);
        void UpdateMaterialCatID(List<Guid> MatIDList, Guid CatID, Guid LinkMainCID);
        void UpdateSupplyMaterial(Guid MatID, decimal CostPrice, decimal MarketPrice, int LeadTime, Guid LinkSupID, string UserFullName);
        void UpdateMaterilMoreDetail(Guid MatID, string MoreDetail);
        void UpdateMaterialImg(Guid MatID, string MatSourceImgPath, string MatImgPath, string MatThumbImgPath);
        void UpdateMatFilePath(Guid MatID, string MatFilePath);
        void DeleteMaterialItem(Guid MatID);
        void DeleteMatFile(Guid MatID);
        void DeleteMatImg(Guid MatID);

        void MatIsPublic(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNotPublic(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsSpotGood(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNotSpotGood(List<Guid> MatIDList, Guid LinkMainCID);

        void MatIsTop(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNew(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNotTop(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNotNew(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsNotPromotion(List<Guid> MatIDList, Guid LinkMainCID);
        void MatIsPromotion(List<Guid> MatIDList, DateTime IsPromotionEndDate, int IsPromotionRate, Guid LinkMainCID);

        void LinkGroupMatList(List<Guid> MatIDList, Guid SourceMatID);
        void NotLinkGroupMatList(List<Guid> MatIDList);
        void LinkGroupMatClone(Guid SourceMatID);

        Material GetMaterialItem(Guid MatID, Guid CID);
        Material GetMaterialItemDB(Guid MatID);

        List<MatExcel> GetMatExcelList(Guid LinkMainCID);
        void InputMaterialByExcel(HttpPostedFileBase ExcelFile, Guid LinkMainCID);
        List<MatExcel> InputMaterialByPurPriceAndLeadTimeExcel(HttpPostedFileBase ExcelFile, Guid LinkMainCID);
        void DeleteMatExcelDBItem(Guid LinkMainCID);
        void MatExcelDBToMaterial(Guid LinkMainCID, Guid MatBrandID, string DefBrandName, string DefBrandType);
        void AutoCallBackPromotion();
    }

    public class MaterialBase : IMaterial
    {
        private EBMSdbContext db = new EBMSdbContext();

        public MaterialPageBySearchList FindMaterialPageBySearchListByQDT(int PageIndex, int PageSize, string Keyword, string MatBrand, Guid CatID, string IsStatUs)
        {
            var query = (from M in db.Material
                         where true
                         select M).AsQueryable();

            //品牌判断
            if (!string.IsNullOrEmpty(MatBrand))
            {
                query = query.Where(m => m.MatBrand.Contains(MatBrand)).AsQueryable();
            }

            ICategory ICat = new CategoryBase();
            CatTree CT = ICat.GetCatTreeByQDT();

            //分类判断
            if (CatID != Guid.Empty)
            {
                if (CT.SecondCatTree.Where(c => c.CatID == CatID).Count() > 0)
                {
                    query = query.Where(c => c.LinkQDTCatID == CatID).AsQueryable();
                }
            }


            //发布 新品 置顶判断
            if (IsStatUs == MatISNo.IsPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();

            }
            else if (IsStatUs == MatISNo.IsNotNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 1 && x.IsGroupMat <= 0).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 0 && x.IsGroupMat <= 0).AsQueryable();
            }

            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }


            //获取以过滤的品牌项
            var queryBrand = query.Where(x => x.MatBrandID != Guid.Empty).GroupBy(x => x.MatBrandID).Select(x => x.Key).AsQueryable();
            List<P_Brand> PBL = new List<P_Brand>();
            P_Brand PB = new P_Brand();
            Brand B = new Brand();

            foreach (var x in queryBrand)
            {
                B = db.Brand.Find(x);
                if (B != null)
                {
                    PB = new P_Brand();
                    PB.BID = B.BID;
                    PB.BrandName = B.BrandName;
                    PB.BrandEnName = B.BrandNameEn;
                    PB.BrandLogo = B.BrandLogo;
                    PBL.Add(PB);
                }else{
                    PB = new P_Brand();
                    PB.BID = Guid.Empty;
                    PB.BrandName = string.Empty;
                    PB.BrandEnName = string.Empty;
                    PB.BrandLogo = string.Empty;
                    PBL.Add(PB);
                }
            }


            List<P_Brand> PBLNew = new List<P_Brand>();
            var QueryBrandNew = from x in PBL
                                where true
                                &&
                                x.BID != Guid.Empty
                                &&
                                x.BrandLogo != string.Empty
                                &&
                                x.BrandName != string.Empty
                                group x by x.BrandName into g
                                select new {
                                    BID = g.FirstOrDefault().BID,
                                    BrandLogo = g.FirstOrDefault().BrandLogo,
                                    BrandName = g.FirstOrDefault().BrandName,
                                    BrandEnName = g.FirstOrDefault().BrandEnName,
                                };
            P_Brand NewB = new P_Brand();
           foreach(var x in QueryBrandNew)
           {
               NewB = new P_Brand();
               NewB.BID = x.BID;
               NewB.BrandLogo = x.BrandLogo;
               NewB.BrandEnName = x.BrandEnName;
               NewB.BrandName = x.BrandName;
               PBLNew.Add(NewB);
           }



            //获取以过滤分类项
            var queryCat = from x in query
                           where true
                           &&
                           x.CategoryID != Guid.Empty
                           &&
                           x.LinkQDTCatID != Guid.Empty
                           group x by x.LinkQDTCatID into g
                           select new
                           {
                               CID = g.Key,
                               Count = g.Count()
                           };



            List<P_Cat> PCL = new List<P_Cat>();
            P_Cat PC = new P_Cat();
            Category Cat = new Category();

            foreach (var x in queryCat)
            {
                PC = new P_Cat();
                if (CT.SecondCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.SecondCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
            }

            List<Material> ML = query.OrderByDescending(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            List<Material> SubGroupML = new List<Material>();

            IBrand IB = new BrandBase();
            string MaxPrice = string.Empty;
            string MinPrice = string.Empty;
            foreach (var x in ML)
            {
                x.MoreDetail = string.Empty;
                if (x.IsGroupMat > 0)
                {
                    SubGroupML = this.GetMaterialByGroupMatID(Guid.Empty, x.MatID);
                    x.LinkGroupMatCount = SubGroupML.Count();
                    try
                    {
                        MaxPrice = SubGroupML.Where(c => c.PriceInfo.SalesPrice > 0).Max(c => c.PriceInfo.SalesPrice).ToString("N");
                        MinPrice = SubGroupML.Where(c => c.PriceInfo.SalesPrice > 0).Min(c => c.PriceInfo.SalesPrice).ToString("N");
                        if (MaxPrice != MinPrice)
                        {
                            x.LinkGroupPrice = MinPrice + " ~ " + MaxPrice;
                        }
                        else
                        {
                            x.LinkGroupPrice = MinPrice;
                        }
                    }
                    catch
                    {
                        x.LinkGroupPrice = string.Empty;
                    }
                }
            }

            ML = IB.GetMatSalesPriceInfoList(ML, Guid.Empty);

            MaterialPageBySearchList MP = new MaterialPageBySearchList();
            MP.PageIndex = PageIndex;
            MP.PageSize = PageSize;
            MP.TotalRecord = query.Count();
            MP.Rows = ML;
            MP.Brand = PBLNew;
            MP.Cat = PCL;
            return MP;
        }

        public MaterialPageBySearchList FindMaterialPageBySearchList(int PageIndex, int PageSize, string Keyword, string MatBrand, Guid MatBrandID, Guid CatID, string IsStatUs, Guid CID, Guid LinkMainCID)
        {
            var query = (from M in db.Material
                         where true
                         &&
                         M.LinkMainCID.Equals(LinkMainCID)
                         select M).AsQueryable();

            //品牌判断
            if (!string.IsNullOrEmpty(MatBrand))
            {
                query = query.Where(m => m.MatBrand.Contains(MatBrand)).AsQueryable();
            }

            //品牌ID判断
            if (MatBrandID != Guid.Empty)
            {
                query = query.Where(m => m.MatBrandID.Equals(MatBrandID)).AsQueryable();
            }


            ICategory ICat = new CategoryBase();
            CatTree CT = ICat.GetCatTree(LinkMainCID);

            //分类判断
            if (CatID != Guid.Empty)
            {
                string TopCatName = string.Empty;
                string SecondCatName = string.Empty;
                //循环分类ID，并识别几级目录树
                if (CT.TopCatTree.Where(c => c.CatID == CatID).Count() > 0)
                {
                    //头部通配
                    TopCatName = CT.TopCatTree.Where(c => c.CatID == CatID).FirstOrDefault().CategoryName;
                    query = query.Where(c => c.CategoryName.StartsWith(TopCatName)).AsQueryable();
                }
                else if (CT.SecondCatTree.Where(c => c.CatID == CatID).Count() > 0)
                {
                    Category C = CT.SecondCatTree.Where(c => c.CatID == CatID).FirstOrDefault();
                    SecondCatName = C.CategoryName;
                    TopCatName = CT.TopCatTree.Where(c => c.CatID == C.ParentCatID).FirstOrDefault().CategoryName;
                    //头部通配
                    query = query.Where(c => c.CategoryName.StartsWith(TopCatName + "," + SecondCatName + ",")).AsQueryable();

                }
                else if (CT.EndCatTree.Where(c => c.CatID == CatID).Count() > 0)
                {
                    //直接指向三级分类ID;
                    query = query.Where(x => x.CategoryID == CatID).AsQueryable();
                }
            }


            //发布 新品 置顶判断
            if (IsStatUs == MatISNo.IsPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();

            }
            else if (IsStatUs == MatISNo.IsNotNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 1 && x.IsGroupMat <= 0).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 0 && x.IsGroupMat <= 0).AsQueryable();
            }

            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }


            //获取以过滤的品牌项
            var queryBrand = query.Where(x => x.MatBrandID != Guid.Empty).GroupBy(x => x.MatBrandID).Select(x => x.Key).AsQueryable();
            List<P_Brand> PBL = new List<P_Brand>();
            P_Brand PB = new P_Brand();
            Brand B = new Brand();

            foreach (var x in queryBrand)
            {
                B = db.Brand.Find(x);
                if(B!= null)
                {
                    PB = new P_Brand();
                    PB.BID = B.BID;
                    PB.BrandName = B.BrandName;
                    PB.BrandEnName = B.BrandNameEn;
                    PB.BrandLogo = B.BrandLogo;
                    PBL.Add(PB);
                }
            }

            //获取以过滤分类项
            var queryCat = from x in query
                           where true
                           &&
                           x.CategoryID != Guid.Empty
                           group x by x.CategoryID into g
                           select new
                           {
                               CID = g.Key,
                               Count = g.Count()
                           };



            List<P_Cat> PCL = new List<P_Cat>();
            P_Cat PC = new P_Cat();
            Category Cat = new Category();

            foreach (var x in queryCat)
            {
                PC = new P_Cat();
                if(CT.TopCatTree.Where(c=>c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.TopCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
                else if (CT.SecondCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.SecondCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
                else if (CT.EndCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.EndCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
            }

            List<Material> ML = query.OrderByDescending(s => s.IsTop).ThenByDescending(s => s.IsNew).ThenBy(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            List<Material> SubGroupML = new List<Material>();

            IBrand IB = new BrandBase();
            string MaxPrice = string.Empty;
            string MinPrice = string.Empty;
            foreach(var x in ML)
            {
                x.MoreDetail = string.Empty;
                if (x.IsGroupMat > 0)
                {
                    SubGroupML = this.GetMaterialByGroupMatID(Guid.Empty, x.MatID);
                    x.LinkGroupMatCount = SubGroupML.Count();
                    try
                    {
                        MaxPrice = SubGroupML.Where(c => c.PriceInfo.SalesPrice > 0).Max(c => c.PriceInfo.SalesPrice).ToString("N");
                        MinPrice = SubGroupML.Where(c => c.PriceInfo.SalesPrice > 0).Min(c => c.PriceInfo.SalesPrice).ToString("N");
                        if (MaxPrice != MinPrice)
                        {
                            x.LinkGroupPrice = MinPrice + " ~ " + MaxPrice;
                        }
                        else
                        {
                            x.LinkGroupPrice = MinPrice;
                        }
                    }
                    catch
                    {
                        x.LinkGroupPrice = string.Empty;
                    }
                }
            }

            ML = IB.GetMatSalesPriceInfoList(ML, CID);

            MaterialPageBySearchList MP = new MaterialPageBySearchList();
            MP.PageIndex = PageIndex;
            MP.PageSize = PageSize;
            MP.TotalRecord = query.Count();
            MP.Rows = ML;
            MP.Brand = PBL;
            MP.Cat = PCL;

            return MP;
        }

        public MaterialPageBySearchList FindMaterialPageByIsGroupSearchList(int PageIndex, int PageSize, string Keyword, Guid LinkMainCID)
        {
            var query = (from M in db.Material
                         where true
                         &&
                         M.LinkMainCID.Equals(LinkMainCID)
                         &&
                         M.IsGroupMat == 1
                         select M).AsQueryable();


            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }


            //获取以过滤的品牌项
            var queryBrand = query.Where(x => x.MatBrandID != Guid.Empty).GroupBy(x => x.MatBrandID).Select(x => x.Key).AsQueryable();
            List<P_Brand> PBL = new List<P_Brand>();
            P_Brand PB = new P_Brand();
            Brand B = new Brand();

            foreach (var x in queryBrand)
            {
                B = db.Brand.Find(x);
                if (B != null)
                {
                    PB = new P_Brand();
                    PB.BID = B.BID;
                    PB.BrandName = B.BrandName;
                    PB.BrandEnName = B.BrandNameEn;
                    PB.BrandLogo = B.BrandLogo;
                    PBL.Add(PB);
                }
            }

            //获取以过滤分类项
            var queryCat = from x in query
                           where true
                           &&
                           x.CategoryID != Guid.Empty
                           group x by x.CategoryID into g
                           select new
                           {
                               CID = g.Key,
                               Count = g.Count()
                           };

            ICategory IC = new CategoryBase();
            CatTree CT = IC.GetCatTree(LinkMainCID);
            List<P_Cat> PCL = new List<P_Cat>();
            P_Cat PC = new P_Cat();
            Category Cat = new Category();

            foreach (var x in queryCat)
            {
                PC = new P_Cat();
                if (CT.TopCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.TopCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
                else if (CT.SecondCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.SecondCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
                else if (CT.EndCatTree.Where(c => c.CatID == x.CID).Count() > 0)
                {
                    PC.CatID = x.CID;
                    PC.CatCount = x.Count;
                    PC.CatName = CT.EndCatTree.Where(c => c.CatID == x.CID).FirstOrDefault().CategoryName;
                    PCL.Add(PC);
                }
            }

            List<Material> ML = query.OrderByDescending(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            foreach (var x in ML)
            {
                x.MoreDetail = string.Empty;
                x.LinkGroupMatCount = db.Material.Where(c => c.GroupToMatID == x.MatID).Count();
            }

            MaterialPageBySearchList MP = new MaterialPageBySearchList();
            MP.PageIndex = PageIndex;
            MP.PageSize = PageSize;
            MP.TotalRecord = query.Count();
            MP.Rows = ML;
            MP.Brand = PBL;
            MP.Cat = PCL;

            return MP;
        }

        public List<Material> FindMaterialByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, string MatBrand, string CatName, string IsImg, string IsStore, Guid MatBrandID, string IsStatUs, Guid LinkMainCID)
        {
            var query = (from M in db.Material
                         where true
                         &&
                         M.LinkMainCID.Equals(LinkMainCID)
                         select M).AsQueryable();

            //品牌判断
            if (!string.IsNullOrEmpty(MatBrand))
            {
                query = query.Where(m => m.MatBrand.Contains(MatBrand)).AsQueryable();
            }

            //品牌ID判断
            if (MatBrandID != Guid.Empty)
            {
                query = query.Where(m => m.MatBrandID.Equals(MatBrandID)).AsQueryable();
            }

            //分类判断
            if (!string.IsNullOrEmpty(CatName))
            {
                if(CatName == "IsNotConfigCatID")
                {
                    query = query.Where(m => m.CategoryID == Guid.Empty).AsQueryable();
                }
                else
                {
                    CatName = CatName + ',';
                    query = query.Where(m => m.CategoryName.Contains(CatName)).AsQueryable();
                }

            }



            //图片判断
            if (IsImg == "IsImg")
            {
                query = query.Where(m => m.MatImgPath != string.Empty && m.MatImgPath != null).AsQueryable();
            }

            if (IsImg == "IsNotImg")
            {
                query = query.Where(m => m.MatImgPath == string.Empty || m.MatImgPath == null).AsQueryable();
            }

            //安全库存判断
            if (IsStore == "IsStore")
            {
                query = query.Where(m => m.SafetyStock > 0).AsQueryable();
            }

            //发布 新品 置顶判断
            if(IsStatUs == MatISNo.IsPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotTop.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsTop == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 1 && x.GroupToMatID == Guid.Empty).AsQueryable();

            }
            else if (IsStatUs == MatISNo.IsNotNew.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsNew == 0 && x.GroupToMatID == Guid.Empty).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 1 && x.IsGroupMat <= 0).AsQueryable();
            }
            else if (IsStatUs == MatISNo.IsNotPromotion.ToString())
            {
                query = query.Where(x => x.IsPublic == 1 && x.IsPromotion == 0 && x.IsGroupMat <= 0 ).AsQueryable();
            }

            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }

            TotalRecord = query.Count();
            List<Material> ML = query.OrderByDescending(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

            foreach(var x in ML)
            {
                try
                {
                    x.SingleOrderNumber = Convert.ToInt32(x.WeeklySales * (x.LeadTime / 7) + x.SafetyStock);
                }
                catch
                {
                    x.SingleOrderNumber = 0;
                }
            }

            string SupName = string.Empty;
            foreach(var x in ML.GroupBy(x=>x.LinkSupID).Select(x=>x.Key))
            {
                if(x == Guid.Empty)
                {
                    SupName = string.Empty;
                }
                else
                {
                    SupName = db.Supplier.Find(x).SupName;
                }

                foreach(var xx in ML.Where(c=>c.LinkSupID == x))
                {
                    xx.SupplierName = SupName;
                }
            }

            IBrand IB = new BrandBase();
            ML = IB.GetMatSalesPriceInfoList(ML, Guid.Empty);
            return ML;
        }

        public List<Material> FindMaterialIsSpotGoodByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, Guid MatBrandID, int IsSpotGood, Guid LinkMainCID)
        {
            var query = (from M in db.Material
                         where true
                         &&
                         M.LinkMainCID.Equals(LinkMainCID)
                         &&
                         M.IsGroupMat <= 0
                         select M).AsQueryable();

            //品牌ID判断
            if (MatBrandID != Guid.Empty)
            {
                query = query.Where(m => m.MatBrandID.Equals(MatBrandID)).AsQueryable();
            }


            if (IsSpotGood == 1)
            {
                query = query.Where(x => x.IsSpotGood == 1).AsQueryable();
            }
            
            
            if (IsSpotGood == 0)
            {
                query = query.Where(x => x.IsSpotGood == 0).AsQueryable();
            }

          

            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }

            TotalRecord = query.Count();
            List<Material> ML = query.OrderByDescending(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            return ML;
        }


        public List<Material> FindMaterialForCustomerByPage(int PageIndex, int PageSize, out int TotalRecord, string Keyword, string MatBrand, string CatName, Guid MatBrandID, string IsStatUs, Guid CustomerID, Guid LinkMainCID)
        {
            var query = (from M in db.Material
                         where true
                         &&
                         M.LinkMainCID.Equals(LinkMainCID)
                         &&
                         M.IsGroupMat <= 0
                         select M).AsQueryable();

            if (IsStatUs == MatISNo.IsPublic.ToString())
            {
                query = query.Where(x => x.IsPublic == 1).AsQueryable();
            }

            //品牌判断
            if (!string.IsNullOrEmpty(MatBrand))
            {
                query = query.Where(m => m.MatBrand.Contains(MatBrand)).AsQueryable();
            }

            //品牌ID判断
            if (MatBrandID != Guid.Empty)
            {
                query = query.Where(m => m.MatBrandID.Equals(MatBrandID)).AsQueryable();
            }

            //分类判断
            if (!string.IsNullOrEmpty(CatName))
            {
                if (CatName == "IsNotConfigCatID")
                {
                    query = query.Where(m => m.CategoryID == Guid.Empty).AsQueryable();
                }
                else
                {
                    CatName = CatName + ',';
                    query = query.Where(m => m.CategoryName.Contains(CatName)).AsQueryable();
                }
            }


            if (!string.IsNullOrEmpty(Keyword))
            {
                Keyword = Regex.Replace(Keyword, @"( )\1+", "$1", RegexOptions.None);
                string[] KeywordStr = Keyword.Split(' ');

                foreach (var K in KeywordStr)
                {
                    if (!string.IsNullOrEmpty(K))
                    {
                        query = query.Where(
                           m => m.MatSn.Contains(K) ||
                           m.MatBrand.Contains(K) ||
                           m.MatManufacturerSn.Contains(K) ||
                           m.MatSpecifications.Contains(K) ||
                           m.MatName.Contains(K) ||
                           m.KeywordIndex.Contains(K)
                           ).AsQueryable();
                    }
                }
            }

            TotalRecord = query.Count();
            List<Material> ML = query.OrderByDescending(s => s.LastUpdateTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            foreach (var x in ML)
            {
                x.MoreDetail = string.Empty;
            }

            IBrand IB = new BrandBase();
            ML = IB.GetMatSalesPriceInfoList(ML, CustomerID);
            return ML;
        }

        public List<Material> GetMaterialByGroupMatID(Guid CID, Guid MatID)
        {
            List<Material> ML = new List<Material>();
            ML = db.Material.Where(x => x.GroupToMatID == MatID).OrderBy(x => x.MatName).ThenBy(x => x.MatSpecifications).ThenBy(x => x.MatManufacturerSn).ToList();

            IBrand IB = new BrandBase();
            ML = IB.GetMatSalesPriceInfoList(ML, CID);
            foreach (var x in ML)
            {
                x.MoreDetail = string.Empty;
            }
            return ML;
        }

        public Guid CreateBaseMaterial(Material M, string UserFullName, Guid LinkMainCID)
        {
            M.MatID = Guid.NewGuid();
            M.MatName = M.MatName.Trim();
            try
            {
                M.MatSpecifications = M.MatSpecifications.Trim();
            }
            catch
            {
                M.MatSpecifications = string.Empty;
            }

            Brand B = db.Brand.Find(M.MatBrandID);

            if (B != null)
            {

                M.LinkSupID = B.LinkSupID;
                M.MatBrandID = B.BID;
                M.MatBrand = B.BrandName;
            }
            else
            {
                M.LinkSupID = Guid.Empty;
                M.MatBrandID = Guid.Empty;
                M.MatBrand = string.Empty;
            }       


            try
            {
                M.MatManufacturerSn = M.MatManufacturerSn.Trim();
            }
            catch
            {
                M.MatManufacturerSn = string.Empty;
            }

            M.MatUnit = M.MatUnit.Trim();

            try
            {
                M.MatSummary = M.MatSummary.Trim();
            }
            catch
            {
                M.MatSummary = string.Empty;
            }


            if (!string.IsNullOrEmpty(M.MatSn))
            {
                CheckMatSnIsValid(M.MatSn, M.MatID, LinkMainCID);
            }
            else
            {
                M.MatSn = this.AutoCreateMatSn(string.Empty, M.MatID, LinkMainCID);
            }

            this.CheckMatBaseInfo(M.MatName, M.MatSpecifications, M.MatUnit, M.MatBrandID, M.MatManufacturerSn, M.MatID, LinkMainCID);

            M.CategoryID = Guid.Empty;
            M.CategoryName = string.Empty;
            M.MatImgPath = string.Empty;
            M.MatSourceImgPath = string.Empty;
            M.MatThumbImgPath = string.Empty;
            M.MatFilePath = string.Empty;
            M.MoreDetail = string.Empty;
            M.CreateTime = DateTime.Now;
            M.LastUpdateTime = DateTime.Now;
            M.IsPromotionEndDate = DateTime.Now;
            M.LastUpdateStr = UserFullName;
            M.LeadTime = 0;
            M.SafetyStock = 0;
            M.WeeklySales = 0;
            M.KeywordIndex = M.MatName + " " + M.MatSpecifications + " " + M.MatBrand + " " + M.MatManufacturerSn;
            M.LinkMainCID = LinkMainCID;
            db.Material.Add(M);
            db.SaveChanges();
            return M.MatID;
        }

        public Guid CreateBaseGroupMaterial(Material M, string UserFullName, Guid LinkMainCID)
        {
            M.MatID = Guid.NewGuid();
            M.MatName = M.MatName.Trim();
            try
            {
                M.MatSpecifications = M.MatSpecifications.Trim();
            }
            catch
            {
                M.MatSpecifications = string.Empty;
            }

            Brand B = db.Brand.Find(M.MatBrandID);

            if (B != null)
            {

                M.LinkSupID = B.LinkSupID;
                M.MatBrandID = B.BID;
                M.MatBrand = B.BrandName;
            }
            else
            {
                M.LinkSupID = Guid.Empty;
                M.MatBrandID = Guid.Empty;
                M.MatBrand = string.Empty;
            }


            try
            {
                M.MatManufacturerSn = M.MatManufacturerSn.Trim();
            }
            catch
            {
                M.MatManufacturerSn = string.Empty;
            }

            M.MatUnit = M.MatUnit.Trim();

            try
            {
                M.MatSummary = M.MatSummary.Trim();
            }
            catch
            {
                M.MatSummary = string.Empty;
            }


            if (!string.IsNullOrEmpty(M.MatSn))
            {
                CheckMatSnIsValid(M.MatSn, M.MatID, LinkMainCID);
            }
            else
            {
                M.MatSn = this.AutoCreateMatSn(string.Empty, M.MatID, LinkMainCID);
            }

            this.CheckMatBaseInfo(M.MatName, M.MatSpecifications, M.MatUnit, M.MatBrandID, M.MatManufacturerSn, M.MatID, LinkMainCID);

            M.CategoryID = Guid.Empty;
            M.CategoryName = string.Empty;
            M.MatImgPath = string.Empty;
            M.MatSourceImgPath = string.Empty;
            M.MatThumbImgPath = string.Empty;
            M.MatFilePath = string.Empty;
            M.MoreDetail = string.Empty;
            M.CreateTime = DateTime.Now;
            M.LastUpdateTime = DateTime.Now;
            M.IsPromotionEndDate = DateTime.Now;
            M.LastUpdateStr = UserFullName;
            M.LeadTime = 0;
            M.SafetyStock = 0;
            M.WeeklySales = 0;
            M.KeywordIndex = M.MatName + " " + M.MatSpecifications + " " + M.MatBrand + " " + M.MatManufacturerSn;
            M.IsGroupMat = 1;
            M.GroupToMatID = Guid.Empty;
            M.LinkMainCID = LinkMainCID;
            db.Material.Add(M);
            db.SaveChanges();
            return M.MatID;
        }

        public void LinkGroupMatClone(Guid SourceMatID)
        {
            Material SourceM = db.Material.Find(SourceMatID);
            foreach (var x in db.Material.Where(x => x.GroupToMatID == SourceM.MatID))
            {
                x.MatImgPath = SourceM.MatImgPath;
                x.MatSourceImgPath = SourceM.MatSourceImgPath;
                x.MatThumbImgPath = SourceM.MatThumbImgPath;
                x.MatFilePath = SourceM.MatFilePath;
                x.MoreDetail = SourceM.MoreDetail;
                x.MatSummary = SourceM.MatSummary;
                db.Entry(x).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void LinkGroupMatList(List<Guid> MatIDList, Guid SourceMatID)
        {
            Material M = new Material();
            foreach(var x in MatIDList)
            {
                M = db.Material.Find(x);
                M.GroupToMatID = SourceMatID;
                db.Entry(M).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void NotLinkGroupMatList(List<Guid> MatIDList)
        {
            Material M = new Material();
            foreach (var x in MatIDList)
            {
                M = db.Material.Find(x);
                M.GroupToMatID = Guid.Empty;
                db.Entry(M).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void InputMaterialByExcel(HttpPostedFileBase ExcelFile, Guid LinkMainCID)
        {
            MyNormalUploadFile MF = new MyNormalUploadFile();
            //创建上传文件
            string ExcelFilePath = MF.NormalUpLoadFileProcess(ExcelFile, "Excel");

            //根据路径通过已存在的excel来创建HSSFWorkbook，即整个excel文档
            XSSFWorkbook workbook = new XSSFWorkbook(new FileStream(HttpRuntime.AppDomainAppPath.ToString() + ExcelFilePath, FileMode.Open, FileAccess.Read));

            //获取excel的第一个sheet
            ISheet sheet = workbook.GetSheetAt(0);

            List<MatExcel> ML = new List<MatExcel>();
            MatExcel M = new MatExcel();

            int Seconds = 0;

            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                Seconds = Seconds + 1;
                IRow row = sheet.GetRow(i);
                M = M = new MatExcel();

                try
                {
                    M.MatSn = row.GetCell(0).ToString().Trim();
                }
                catch
                {
                    M.MatSn = string.Empty;
                }


                try
                {
                    M.MatName = row.GetCell(1).ToString().Trim();
                }
                catch
                {
                    M.MatName = string.Empty;
                }

                try
                {
                    M.MatSpecifications = row.GetCell(2).ToString().Trim();
                }
                catch
                {
                    M.MatSpecifications = string.Empty;
                }

                try
                {
                    M.MatManufacturerSn = row.GetCell(3).ToString().Trim();
                }
                catch
                {
                    M.MatManufacturerSn = string.Empty;
                }

                try
                {
                    M.MatBrand = row.GetCell(4).ToString().Trim();
                }
                catch
                {
                    M.MatBrand = string.Empty;
                }

                try
                {
                    M.MatBrandType = row.GetCell(5).ToString().Trim();
                }
                catch
                {
                    M.MatBrandType = string.Empty;
                }


                try
                {
                    M.MatUnit = row.GetCell(6).ToString().Trim();
                }
                catch
                {
                    M.MatUnit = string.Empty;
                }
           

                try
                {
                    M.CostPrice = Convert.ToDecimal(row.GetCell(7).ToString().Trim());
                }
                catch
                {
                    M.CostPrice = 0;
                }

                try
                {
                    M.MarketPrice = Convert.ToDecimal(row.GetCell(8).ToString().Trim());
                }
                catch
                {
                    M.MarketPrice = 0;
                }

                try
                {
                    M.LeadTime = Convert.ToInt32(row.GetCell(9).ToString().Trim());
                }
                catch
                {
                    M.LeadTime = 0;
                }

                try
                {
                    M.MatSummary = row.GetCell(10).ToString().Trim();
                }
                catch
                {
                    M.MatSummary = string.Empty;
                }

                if (Seconds > 1 && Seconds <= 1001 && !string.IsNullOrEmpty(M.MatName) && !string.IsNullOrEmpty(M.MatUnit) && !string.IsNullOrEmpty(M.MatBrand))
                {
                    ML.Add(M);
                }
            }




            ////删除持久化内容
            try {
                db.MatExcelDB.Remove(db.MatExcelDB.Find(LinkMainCID));
                db.SaveChanges();            
            }
            catch
            {
                //Dothing
            }

            //存储进入持久化Json数据
            string JSONStr = JsonConvert.SerializeObject(ML);
            MatExcelDB MExcel = new MatExcelDB();
            MExcel.LinkMainCID = LinkMainCID;
            MExcel.MatListJson = JSONStr;
            db.MatExcelDB.Add(MExcel);
            db.SaveChanges();
        }

        public List<MatExcel> InputMaterialByPurPriceAndLeadTimeExcel(HttpPostedFileBase ExcelFile, Guid LinkMainCID)
        {

            MyNormalUploadFile MF = new MyNormalUploadFile();
            //创建上传文件
            string ExcelFilePath = MF.NormalUpLoadFileProcess(ExcelFile, "Excel");

            //根据路径通过已存在的excel来创建HSSFWorkbook，即整个excel文档
            XSSFWorkbook workbook = new XSSFWorkbook(new FileStream(HttpRuntime.AppDomainAppPath.ToString() + ExcelFilePath, FileMode.Open, FileAccess.Read));

            //获取excel的第一个sheet
            ISheet sheet = workbook.GetSheetAt(0);

            List<MatExcel> ML = new List<MatExcel>();
            MatExcel M = new MatExcel();

            int Seconds = 0;

            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                Seconds = Seconds + 1;
                IRow row = sheet.GetRow(i);
                M = M = new MatExcel();

                try
                {
                    M.MatID = new Guid(row.GetCell(0).ToString().Trim());
                }
                catch
                {
                    M.MatID = Guid.Empty;
                }

                try
                {
                    M.MatSn = row.GetCell(2).ToString().Trim();
                }
                catch
                {
                    M.MatSn = string.Empty;
                }


                try
                {
                    M.MatName = row.GetCell(3).ToString().Trim();
                }
                catch
                {
                    M.MatName = string.Empty;
                }

                try
                {
                    M.MatSpecifications = row.GetCell(4).ToString().Trim();
                }
                catch
                {
                    M.MatSpecifications = string.Empty;
                }

                try
                {
                    M.MatBrand = row.GetCell(5).ToString().Trim();
                }
                catch
                {
                    M.MatBrand = string.Empty;
                }


                try
                {
                    M.MatManufacturerSn = row.GetCell(6).ToString().Trim();
                }
                catch
                {
                    M.MatManufacturerSn = string.Empty;
                }

                try
                {
                    M.MatUnit = row.GetCell(7).ToString().Trim();
                }
                catch
                {
                    M.MatUnit = string.Empty;
                }


                try
                {
                    M.CostPrice = Convert.ToDecimal(row.GetCell(8).ToString().Trim());
                }
                catch
                {
                    M.CostPrice = 0;
                }

                try
                {
                    M.MarketPrice = Convert.ToDecimal(row.GetCell(9).ToString().Trim());
                }
                catch
                {
                    M.MarketPrice = 0;
                }

                try
                {
                    M.LeadTime = Convert.ToInt32(row.GetCell(10).ToString().Trim());
                }
                catch
                {
                    M.LeadTime = 0;
                }


                try
                {
                    M.SupplierName = row.GetCell(11).ToString().Trim();
                }
                catch
                {
                    M.SupplierName = string.Empty;
                }

                M.IsErrorMatInfo = 0;
                M.IsErrorMatSn = 0;

                if (Seconds > 1 && Seconds <= 4001 && M.MatID != Guid.Empty && !string.IsNullOrEmpty(M.MatName))
                {
                    ML.Add(M);
                }
            }
            return ML;
        }

        public void MatExcelDBToMaterial(Guid LinkMainCID, Guid MatBrandID, string DefBrandName, string DefBrandType)
        {
            List<MatExcel> EML = this.GetMatExcelList(LinkMainCID);
            Material M = new Material();

            string BrandName = string.Empty;
            string BrandType = string.Empty;

            Brand B = db.Brand.Find(MatBrandID);

            try
            {
                BrandName = B.BrandName;
                BrandType = B.BrandType;
            }
            catch
            {
                BrandName = string.Empty;
                BrandType = string.Empty;
            }

            if (BrandName != DefBrandName || BrandType != DefBrandType)
            {
                throw new Exception("错误！ - 所选品牌与导入品牌不一致！");
            }

            DateTime Now = DateTime.Now;
            foreach (var x in EML)
            {
                M = new Material();
                M.MatID = Guid.NewGuid();
                M.MatSn = this.AutoCreateMatSn(string.Empty, M.MatID, LinkMainCID);
                M.MatName = x.MatName;
                M.MatSpecifications = x.MatSpecifications;
                M.MatManufacturerSn = x.MatManufacturerSn;
                M.MatUnit = x.MatUnit;
                M.MatBrandID = B.BID;
                M.MatBrand = B.BrandName;
                M.MatSummary = x.MatSummary;
                M.MoreDetail = string.Empty;
                M.CreateTime = Now;
                M.LastUpdateTime = Now;
                M.IsPromotionEndDate = Now;
                M.LastUpdateStr = string.Empty;
                M.CategoryID = Guid.Empty;
                M.CategoryName = string.Empty;
                M.MatImgPath = string.Empty;
                M.MatSourceImgPath = string.Empty;
                M.MatThumbImgPath = string.Empty;
                M.MatFilePath = string.Empty;
                M.CostPrice = x.CostPrice;
                M.MarketPrice = x.MarketPrice;
                M.LeadTime = x.LeadTime;
                M.LinkSupID = B.LinkSupID;
                M.LinkMainCID = LinkMainCID;
                M.SafetyStock = 0;
                M.WeeklySales = 0;
                M.KeywordIndex = x.MatName + " " + x.MatSpecifications + " " + x.MatBrand + " " + x.MatManufacturerSn;
                db.Material.Add(M);
            }

            db.MatExcelDB.Remove(db.MatExcelDB.Find(LinkMainCID));

            string ErrorStr = string.Empty;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var x in ex.EntityValidationErrors)
                {
                    foreach (var xx in x.ValidationErrors)
                    {
                        ErrorStr += xx.ErrorMessage.ToString();
                    }
                }
                throw new Exception(ErrorStr);
            }
        }

        public void DeleteMatExcelDBItem(Guid LinkMainCID)
        {
            MatExcelDB M = db.MatExcelDB.Find(LinkMainCID);
            if (M != null)
            {
                db.MatExcelDB.Remove(M);
                db.SaveChanges();
            }
        }

        public List<MatExcel> GetMatExcelList(Guid LinkMainCID)
        {
            List<MatExcel> ML = new List<MatExcel>();
            try
            {
                ML = JsonConvert.DeserializeObject<List<MatExcel>>(db.MatExcelDB.Find(LinkMainCID).MatListJson);
                if(ML == null)
                {
                    ML = new List<MatExcel>();
                }
            }
            catch
            {
                ML = new List<MatExcel>();
            }

            foreach(var x in ML)
            {
                if(!string.IsNullOrEmpty(x.MatSn))
                {
                    try
                    {
                        this.CheckMatSnIsValid(x.MatSn, Guid.NewGuid(), LinkMainCID);
                    }
                    catch
                    {
                        x.IsErrorMatSn = 1;
                    }
                }
                else
                {
                    x.IsErrorMatSn = 0;
                }


                try
                {
                    this.CheckMatBaseInfoByBrand(x.MatName, x.MatSpecifications, x.MatUnit, x.MatBrand, x.MatManufacturerSn, Guid.NewGuid(), LinkMainCID);
                }
                catch
                {
                    x.IsErrorMatInfo = 1;
                }

            }


            return ML;
        }

        public Material GetMaterialItemDB(Guid MatID)
        {
            return db.Material.Find(MatID);
        }

        public Material GetMaterialItem(Guid MatID, Guid CID)
        {
            Material M = db.Material.Find(MatID);
            IBrand IB = new BrandBase();
            M.PriceInfo = IB.GetMatSalesPriceInfo(M, CID);

            try
            {
                M.SupplierName = db.Supplier.Find(M.LinkSupID).SupName;
            }
            catch
            {
                M.SupplierName = string.Empty;
            }

            MainCompany MC = db.MainCompany.Find(M.LinkMainCID);
            if(MC != null)
            {
                M.LinkMainCIDComName = MC.MainCompanyName;
                M.linkMainCIDWebSite = MC.WebSiteName;
            }

            return M;
        }

        public void UpdateBaseMaterial(Material M, string UserFullName)
        {
            Material OldM = db.Material.Find(M.MatID);
            OldM.MatName = M.MatName.Trim();
            try
            {
                OldM.MatSpecifications = M.MatSpecifications.Trim();
            }
            catch
            {
                OldM.MatSpecifications = string.Empty;
            }

            Brand B = db.Brand.Find(M.MatBrandID);

            if (B != null)
            {
                OldM.MatBrandID = B.BID;
                OldM.MatBrand = B.BrandName;
                OldM.LinkSupID = B.LinkSupID;
            }
            else
            {
                OldM.MatBrandID = Guid.Empty;
                OldM.MatBrand = string.Empty;
                OldM.LinkSupID = Guid.Empty;
            }


            try
            {
                OldM.MatManufacturerSn = M.MatManufacturerSn.Trim();
            }
            catch
            {
                OldM.MatManufacturerSn = string.Empty;
            }

            OldM.MatUnit = M.MatUnit.Trim();
            try
            {
                OldM.MatSummary = M.MatSummary.Trim();
            }
            catch
            {
                OldM.MatSummary = string.Empty;
            }

            OldM.KeywordIndex = M.MatName + " " + M.MatSpecifications + " " + M.MatBrand + " " + M.MatManufacturerSn;

            OldM.LastUpdateTime = DateTime.Now;

            this.CheckMatBaseInfo(OldM.MatName, OldM.MatSpecifications, OldM.MatUnit, OldM.MatBrandID, OldM.MatManufacturerSn, OldM.MatID, OldM.LinkMainCID);
            db.Entry(OldM).State = EntityState.Modified;

            string ErrorStr = string.Empty;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var x in ex.EntityValidationErrors)
                {
                    foreach (var xx in x.ValidationErrors)
                    {
                        ErrorStr += xx.ErrorMessage.ToString();
                    }
                }
                throw new Exception(ErrorStr);
            }
        }

        public void UpdateSupplyMaterial(Guid MatID, decimal CostPrice, decimal MarketPrice, int LeadTime, Guid LinkSupID, string UserFullName)
        {
            Material OldM = db.Material.Find(MatID);
            OldM.CostPrice = CostPrice;
            OldM.MarketPrice = MarketPrice;
            OldM.LinkSupID = LinkSupID;
            OldM.LeadTime = LeadTime;
            OldM.LastUpdateTime = DateTime.Now;
            db.Entry(OldM).State = EntityState.Modified;

            string ErrorStr = string.Empty;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var x in ex.EntityValidationErrors)
                {
                    foreach (var xx in x.ValidationErrors)
                    {
                        ErrorStr += xx.ErrorMessage.ToString();
                    }
                }
                throw new Exception(ErrorStr);
            }
        }

        public void UpdateMaterialPriceAndLeadTime(Guid MatID, decimal CostPrice, decimal MarketPrice, int LeadTime)
        {
            Material OldM = db.Material.Find(MatID);
            OldM.CostPrice = CostPrice;
            OldM.MarketPrice = MarketPrice;
            OldM.LeadTime = LeadTime;
            db.Entry(OldM).State = EntityState.Modified;

            string ErrorStr = string.Empty;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var x in ex.EntityValidationErrors)
                {
                    foreach (var xx in x.ValidationErrors)
                    {
                        ErrorStr += xx.ErrorMessage.ToString();
                    }
                }
                throw new Exception(ErrorStr);
            }
        }

        public void UpdateMaterialPriceAndLeadTimeBatch(Guid LinkMainCID, List<MatExcel> ML)
        {
            Material M = new Material();
            foreach(var x in ML)
            {
                M = db.Material.Find(x.MatID);
                M.CostPrice = x.CostPrice;
                M.MarketPrice = x.MarketPrice;
                M.LeadTime = x.LeadTime;
                db.Entry(M).State = EntityState.Modified;
            }
            db.SaveChangesAsync();
        }

        public void UpdateStoreInfo(Guid MatID, int SafetyStock, int WeeklySales)
        {
            Material OldM = db.Material.Find(MatID);
            OldM.SafetyStock = SafetyStock;
            OldM.WeeklySales = WeeklySales;
            db.Entry(OldM).State = EntityState.Modified;

            string ErrorStr = string.Empty;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var x in ex.EntityValidationErrors)
                {
                    foreach (var xx in x.ValidationErrors)
                    {
                        ErrorStr += xx.ErrorMessage.ToString();
                    }
                }
                throw new Exception(ErrorStr);
            }
        }

        public void AutoCallBackPromotion()
        {
            DateTime Now = DateTime.Now;
            if (db.Material.Where(x => x.IsPromotion > 0 && x.IsPromotionEndDate <= Now).Count() > 0)
            {
                foreach (var x in db.Material.Where(x => x.IsPromotion > 0 && x.IsPromotionEndDate <= Now))
                {
                    x.IsPromotion = 0;
                    db.Entry(x).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        public void MatIsPromotion(List<Guid> MatIDList, DateTime IsPromotionEndDate, int IsPromotionRate,  Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                DateTime EndPromotionEndDate = Convert.ToDateTime(IsPromotionEndDate.ToString("yyyy-MM-dd 23:59:59"));

                try
                {
                    M = db.Material.Find(x);
                    M.IsPromotion = 1;
                    M.IsPromotionRate = IsPromotionRate;
                    M.IsPromotionEndDate = EndPromotionEndDate;
                    db.Entry(M).State = EntityState.Modified;
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();

        }

        public void MatIsNotPromotion(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPromotion = 0;
                    M.IsPromotionRate = 0;
                    db.Entry(M).State = EntityState.Modified;
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();

        }

        public void MatIsSpotGood(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsSpotGood = 1;
                    db.Entry(M).State = EntityState.Modified;
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsNotSpotGood(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsSpotGood = 0;
                    db.Entry(M).State = EntityState.Modified;
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsPublic(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 1;
                    db.Entry(M).State = EntityState.Modified;
                    foreach(var xx in db.Material.Where(c=>c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 1;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsNotPublic(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 0;
                    M.IsNew = 0;
                    M.IsTop = 0;
                    db.Entry(M).State = EntityState.Modified;
                    foreach (var xx in db.Material.Where(c => c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 0;
                        xx.IsNew = 0;
                        xx.IsTop = 0;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsTop(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 1;
                    M.IsTop = 1;
                    M.IsNew = 1;
                    M.LastUpdateTime = DateTime.Now;
                    db.Entry(M).State = EntityState.Modified;
                    foreach (var xx in db.Material.Where(c => c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 1;
                        xx.IsTop = 1;
                        xx.IsNew = 1;
                        xx.LastUpdateTime = DateTime.Now;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsNotTop(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 1;
                    M.IsTop = 0;
                    db.Entry(M).State = EntityState.Modified;
                    foreach (var xx in db.Material.Where(c => c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 1;
                        xx.IsTop = 0;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsNew(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 1;
                    M.IsNew = 1;
                    db.Entry(M).State = EntityState.Modified;
                    foreach (var xx in db.Material.Where(c => c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 1;
                        xx.IsNew = 1;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void MatIsNotNew(List<Guid> MatIDList, Guid LinkMainCID)
        {
            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.IsPublic = 1;
                    M.IsNew = 0;
                    db.Entry(M).State = EntityState.Modified;
                    foreach (var xx in db.Material.Where(c => c.GroupToMatID == M.MatID).ToList())
                    {
                        xx.IsPublic = 1;
                        xx.IsNew = 0;
                        db.Entry(xx).State = EntityState.Modified;
                    }
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }

        public void UpdateMaterialCatID(List<Guid> MatIDList, Guid CatID, Guid LinkMainCID)
        {
            string CatNameStr = string.Empty;
            ICategory IC = new CategoryBase();
            CatTree C = IC.GetCatTree(LinkMainCID);


            string NamePath = string.Empty;
            if(C.TopCatTree.Where(x=>x.CatID == CatID).FirstOrDefault() != null)
            {
                NamePath = C.TopCatTree.Where(x => x.CatID == CatID).FirstOrDefault().CategoryNamePath;
            }

            if (C.SecondCatTree.Where(x => x.CatID == CatID).FirstOrDefault() != null)
            {
                NamePath = C.SecondCatTree.Where(x => x.CatID == CatID).FirstOrDefault().CategoryNamePath;
            }

            if (C.EndCatTree.Where(x => x.CatID == CatID).FirstOrDefault() != null)
            {
                NamePath = C.EndCatTree.Where(x => x.CatID == CatID).FirstOrDefault().CategoryNamePath;
            }


            if(CatID == Guid.Empty)
            {
                CatNameStr = string.Empty;
            }
            else
            {
                CatNameStr = NamePath;
            }

            Material M = new Material();

            foreach (var x in MatIDList)
            {
                try
                {
                    M = db.Material.Find(x);
                    M.CategoryID = CatID;
                    M.CategoryName = CatNameStr;
                    db.Entry(M).State = EntityState.Modified;
                }
                catch
                {
                    //Dothing
                }
            }
            db.SaveChanges();
        }


        public void UpdateMaterilMoreDetail(Guid MatID, string MoreDetail)
        {
            Material MT = db.Material.Find(MatID);
            MT.MoreDetail = MoreDetail;
            db.Entry(MT).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void UpdateMaterialImg(Guid MatID, string MatSourceImgPath, string MatImgPath, string MatThumbImgPath)
        {
            Material M = db.Material.Find(MatID);

            //删除原始图片
            string Old_MatSourceImgPath = M.MatSourceImgPath;
            string Old_MatImgPath = M.MatImgPath;
            string Old_MatThumbImgPath = M.MatThumbImgPath;

            if (db.Material.Where(m => m.MatImgPath == Old_MatImgPath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatSourceImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatThumbImgPath);
            }

            //更新新图片
            M.MatSourceImgPath = MatSourceImgPath;
            M.MatImgPath = MatImgPath;
            M.MatThumbImgPath = MatThumbImgPath;
            db.Entry(M).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void UpdateMatFilePath(Guid MatID, string MatFilePath)
        {
            Material M = db.Material.Find(MatID);
            string Old_MatFilePath = M.MatFilePath;
            if (db.Material.Where(m => m.MatFilePath == Old_MatFilePath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatFilePath);
            }
            M.MatFilePath = MatFilePath;
            db.Entry(M).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void DeleteMaterialItem(Guid MatID)
        {
            Material M = db.Material.Find(MatID);
            //删除原始文件
            string Old_MatFilePath = M.MatFilePath;
            if (db.Material.Where(m => m.MatFilePath == Old_MatFilePath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatFilePath);
            }

            //删除原始图片
            string Old_MatSourceImgPath = M.MatSourceImgPath;
            string Old_MatImgPath = M.MatImgPath;
            string Old_MatThumbImgPath = M.MatThumbImgPath;

            if (db.Material.Where(m => m.MatImgPath == Old_MatImgPath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatSourceImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatThumbImgPath);
            }

            //清理群组属性
            foreach(var x in db.Material.Where(x=>x.GroupToMatID == M.MatID))
            {
                x.GroupToMatID = Guid.Empty;
                db.Entry(x).State = EntityState.Modified;
            }


            db.Material.Remove(M);
            db.SaveChanges();
        }

        public void DeleteMatFile(Guid MatID)
        {

            Material M = db.Material.Find(MatID);
            string Old_MatFilePath = M.MatFilePath;
            if (db.Material.Where(m => m.MatFilePath == Old_MatFilePath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatFilePath);
            }
            M.MatFilePath = string.Empty;
            db.Entry(M).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void DeleteMatImg(Guid MatID)
        {
            Material M = db.Material.Find(MatID);

            //删除原始图片
            string Old_MatSourceImgPath = M.MatSourceImgPath;
            string Old_MatImgPath = M.MatImgPath;
            string Old_MatThumbImgPath = M.MatThumbImgPath;

            if (db.Material.Where(m => m.MatImgPath == Old_MatImgPath).Count() < 2)
            {
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatSourceImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatImgPath);
                MyDeleteUploadFile.MyDeleteUploadFileProcess(Old_MatThumbImgPath);
            }

            //更新新图片
            M.MatSourceImgPath = string.Empty;
            M.MatImgPath = string.Empty;
            M.MatThumbImgPath = string.Empty;
            db.Entry(M).State = EntityState.Modified;
            db.SaveChanges();
        }

        private string AutoCreateMatSn(string PrefixStr, Guid MatID, Guid LinkMainCID)
        {
            if (string.IsNullOrEmpty(PrefixStr))
            {
                PrefixStr = "E";
            }

            //初始化数字编码
            string MatSnIntNo = string.Empty;

            //最大系统编码号
            IMaxInt IMax = new MaxIntBase();
            string MaxIntStr = IMax.GetMaxIntByApp(MaxIntAppName.Material.ToString(), LinkMainCID).ToString();

            //数字编码位数设定
            int matSnPadLeftNo = 6;

            //超过预设位数，则截取后n位数字
            if (MaxIntStr.Length >= matSnPadLeftNo)
            {
                //Substring(参数1为左起始位数，参数2为截取几位。)
                MatSnIntNo = MaxIntStr.Substring((MaxIntStr.Length - matSnPadLeftNo), matSnPadLeftNo);
            }
            else
            {
                //用0填充未满部分字符
                MatSnIntNo = MaxIntStr.ToString().PadLeft(matSnPadLeftNo, '0');
            }

            string NewMainSn = PrefixStr + MatSnIntNo;
            this.CheckMatSnIsValid(NewMainSn, MatID, LinkMainCID);
            return NewMainSn; 
        }

        private void CheckMatSnIsValid(string MatSn, Guid MatID, Guid LinkMainCID)
        {
            if(db.Material.Where(x=>x.LinkMainCID == LinkMainCID && x.MatID != MatID && x.MatSn == MatSn).Count() > 0)
            {
                throw new Exception("错误 - 重复的产品编号！");
            }
        }

        private void CheckMatBaseInfo(string MatName, string MatSpecifications, string MatUnit, Guid MatBrandID, string MatManufacturerSn, Guid MatID, Guid LinkMainCID)
        {
            if (db.Material.Where(x => x.LinkMainCID == LinkMainCID && x.MatID != MatID && x.MatName == MatName && x.MatSpecifications == MatSpecifications && x.MatUnit == MatUnit && x.MatBrandID == MatBrandID && x.MatManufacturerSn == MatManufacturerSn).Count() > 0)
            {
                throw new Exception("错误 - 系统已存在重复的产品信息！");
            }
        }

        private void CheckMatBaseInfoByBrand(string MatName, string MatSpecifications, string MatUnit, string MatBrand, string MatManufacturerSn, Guid MatID, Guid LinkMainCID)
        {
            if (db.Material.Where(x => x.LinkMainCID == LinkMainCID && x.MatID != MatID && x.MatName == MatName && x.MatSpecifications == MatSpecifications && x.MatUnit == MatUnit && x.MatBrand == MatBrand && x.MatManufacturerSn == MatManufacturerSn).Count() > 0)
            {
                throw new Exception("错误 - 系统已存在重复的产品信息！");
            }
        }
    }

}
