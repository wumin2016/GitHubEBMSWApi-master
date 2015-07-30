using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBMSW.Models
{
    public interface IClone
    {
        int CloneMaterialToNewMainCom(Guid SourceMainCID, List<Guid> SourceBrandID, Guid ToMainCID);
        List<BrandByMainCom> GetBrandByMainComList(int PageIndex, int PageSize, out int TotalRecord, string Keyword);
    }

    public class CloneBase : IClone
    {
        private EBMSdbContext db = new EBMSdbContext();


        //获取未被克隆的品牌列表
        public List<BrandByMainCom> GetBrandByMainComList(int PageIndex, int PageSize, out int TotalRecord, string Keyword)
        {
            //获取所有未被克隆的品牌
            var query = (from x in db.Material
                         where true
                         && 
                         Keyword == string.Empty ? true : x.MatBrand.Contains(Keyword)
                         &&
                         x.MatBrandID != Guid.Empty
                         &&
                         x.IsClone <= 0
                         group x by x.MatBrandID into g
                         select new {
                             BrandID = g.Key,
                             ProCount = g.Count(),
                             ProIsPriceCount = g.Where(c=>c.CostPrice > 0).Count()
                         }).AsQueryable();
          
            TotalRecord = query.Count();

            List<BrandByMainCom> BML = new List<BrandByMainCom>();
            BrandByMainCom BM = new BrandByMainCom();
            Brand B = new Brand();
            foreach (var x in query.OrderByDescending(s => s.ProCount).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList())
            {
                B = db.Brand.Find(x.BrandID);

                if (B != null)
                {
                    BM = new BrandByMainCom();
                    BM.BID = B.BID;
                    BM.BrandName = B.BrandName;
                    BM.BrandType = B.BrandType;
                    BM.ProCount = x.ProCount;
                    BM.ProIsPriceCount = x.ProIsPriceCount;
                    BM.FromType = "分销商";
                    BM.LinkMainCID = B.LinkMainCID;
                    BM.LinkMainComName = string.Empty;
                    BML.Add(BM);
                }
            }


            MainCompany MCom = new MainCompany();
            foreach(var x in BML.GroupBy(x=>x.LinkMainCID).Select(x=>x.Key))
            {
                MCom = db.MainCompany.Find(x);
                if(MCom != null)
                {
                    foreach (var xx in BML.Where(c => c.LinkMainCID == x).ToList())
                    {
                        xx.LinkMainComName = MCom.MainCompanyName;
                    }
                }
            }
            return BML;
        }


        public int CloneMaterialToNewMainCom(Guid SourceMainCID, List<Guid> SourceBrandID, Guid ToMainCID)
        {

            if (SourceMainCID == Guid.Empty || ToMainCID == Guid.Empty)
            {
                throw new Exception("错误 - 经营主体不能为空！");
            }

            if (SourceMainCID == ToMainCID)
            {
                throw new Exception("错误 - 相同的经营主体！");
            }

            if (SourceBrandID.Count() <= 0)
            {
                throw new Exception("错误 - 未选择品牌！");
            }

            Brand S_Brand = new Brand();
            Brand New_Brand = new Brand();
            List<Brand> S_BList = new List<Brand>();

            //进行品牌的创建
            foreach (var BID in SourceBrandID)
            {
                S_Brand = db.Brand.Find(BID);
                S_BList.Add(S_Brand);
                if (db.Brand.Where(x => x.BrandName == S_Brand.BrandName && x.BrandType == S_Brand.BrandType && x.LinkMainCID == ToMainCID).Count() <= 0)
                {
                    New_Brand = new Brand();
                    New_Brand.BID = Guid.NewGuid();
                    New_Brand.BrandName = S_Brand.BrandName;
                    New_Brand.BrandNameEn = S_Brand.BrandNameEn;
                    New_Brand.BrandLogo = string.Empty;
                    New_Brand.BrandDes = S_Brand.BrandDes;
                    New_Brand.LinkMainCID = ToMainCID;
                    New_Brand.BrandType = S_Brand.BrandType;
                    New_Brand.BrandMyLevelJson = S_Brand.BrandMyLevelJson;
                    New_Brand.LinkSupID = Guid.Empty;
                    New_Brand.CertificateImg = string.Empty;
                    New_Brand.ShowMarkPrice = S_Brand.ShowMarkPrice;
                    db.Brand.Add(New_Brand);
                    db.SaveChanges();
                }
            }


            //获取新经营主体品牌列表
            List<Brand> NewBrandList = db.Brand.Where(x => x.LinkMainCID == ToMainCID).ToList();

            //进行产品的批量克隆
            Guid NewBID = Guid.Empty;
            int Count = 0;
            foreach (var B in S_BList)
            {
                //循环品牌对应源经营主体产品列表
                NewBID = NewBrandList.Where(x => x.BrandName == B.BrandName && x.BrandType == B.BrandType).FirstOrDefault().BID;
                foreach (var m in db.Material.Where(x => x.LinkMainCID == SourceMainCID && x.MatBrandID == B.BID && x.IsPublic == 1).ToList())
                {
                    Count = Count + this.CloneMatItem(m, NewBID, ToMainCID);
                }
            }
            return Count;
        }

        //克隆产品本身信息
        private int CloneMatItem(Material SM, Guid MatBrandID, Guid NewMainComID)
        {
            int IsToDB = 0;

            try
            {
                Guid MatID = Guid.NewGuid();
                if (db.Material.Where(x => x.LinkMainCID == NewMainComID && x.MatID != MatID && x.MatName == SM.MatName && x.MatSpecifications == SM.MatSpecifications && x.MatUnit == SM.MatUnit && x.MatBrandID == MatBrandID && x.MatManufacturerSn == SM.MatManufacturerSn).Count() <= 0)
                {
                    Material NewSM = new Material();
                    NewSM.MatID = MatID;
                    NewSM.MatSn = "C" + SM.MatSn;
                    NewSM.MatName = SM.MatName;
                    NewSM.MatSpecifications = SM.MatSpecifications;
                    NewSM.MatUnit = SM.MatUnit;
                    NewSM.MatBrand = SM.MatBrand;
                    NewSM.MatManufacturerSn = SM.MatManufacturerSn;
                    NewSM.MatThumbImgPath = SM.MatThumbImgPath;
                    NewSM.MatImgPath = SM.MatImgPath;
                    NewSM.MatSourceImgPath = SM.MatSourceImgPath;
                    NewSM.MatFilePath = SM.MatFilePath;
                    NewSM.MoreDetail = SM.MoreDetail;
                    NewSM.MatSummary = SM.MatSummary;
                    NewSM.CreateTime = DateTime.Now;
                    NewSM.LastUpdateTime = DateTime.Now;
                    NewSM.LastUpdateStr = string.Empty;
                    NewSM.CategoryID = SM.CategoryID;
                    NewSM.CategoryName = SM.CategoryName;
                    NewSM.KeywordIndex = SM.KeywordIndex;
                    NewSM.LinkMainCID = NewMainComID;
                    NewSM.MatBrandID = MatBrandID;
                    NewSM.CostPrice = SM.CostPrice;
                    NewSM.MarketPrice = SM.MarketPrice;
                    NewSM.LinkSupID = Guid.Empty;
                    NewSM.LeadTime = 0;
                    NewSM.SafetyStock = 0;
                    NewSM.WeeklySales = 0;
                    NewSM.IsPublic = SM.IsPublic;
                    NewSM.IsNew = SM.IsNew;
                    NewSM.IsTop = SM.IsTop;
                    db.Material.Add(NewSM);
                    db.SaveChanges();
                    IsToDB = 1;
                }

            }
            catch
            {
                IsToDB = 0;
            }

            return IsToDB;
        }

    }





}
