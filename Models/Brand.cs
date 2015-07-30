using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EBMSW.Models
{
    public class Brand
    {
        [Key]
        public Guid BID { get; set; }

        [Required]
        public string BrandName { get; set; }

        [DefaultValue("")]
        public string BrandNameEn { get; set; }

        [DefaultValue("")]
        public string BrandLogo { get; set; }

        [DefaultValue("")]
        public string CertificateImg { get; set; }

        //品牌分类定义
        [DefaultValue("")]
        public string BrandType { get; set; }

        public string BrandDes { get; set; }

        [DefaultValue("")]
        public string BrandMyLevelJson { get; set; }

        [DefaultValue(0)]
        public int ShowMarkPrice { get; set; }

        [Required]
        public Guid LinkMainCID { get; set; }

        [Required]
        [DefaultValue(0)]
        public int ShortOrder { get; set; }

        [DefaultValue("")]
        public string Regional { get; set; }

        [Required]
        public Guid LinkSupID { get; set; }

        [NotMapped]
        public string SupplierName { get; set; }

        [NotMapped]
        public List<BrandLevel> BrandAllLevel { get; set; }

        [NotMapped]
        public List<BrandLevel> BrandMyLevel { get; set; }

    }

    public class BrandGlobalLevel
    {
        [Key]
        public Guid LinkMainCID { get; set; }

        [DefaultValue("")]
        public string BrandLevelJson { get; set; }
    }

    [NotMapped]
    public class BrandLevel
    {
        public string BLCode { get; set; }
        public string BLName { get; set; }
        
        //备注
        public string BLRemark { get; set; }
        
        //折扣率
        public int Rate { get; set; }

        //返利备注
        public string NoteRebate { get; set; }
    }


    [NotMapped]
    public class BrandPageList
    {
        public int TotalRecord { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<Brand> Rows { get; set; }
    }

    [NotMapped]
    public class BrandByMainCom
    {
        public Guid BID { get; set; }

        public string BrandName { get; set; }

        public string BrandNameEn { get; set; }
        //品牌分类定义
        public string BrandType { get; set; }
        //产品数
        public int ProCount { get; set; }
        //来源
        public string FromType { get; set; }
        //有价格数
        public int ProIsPriceCount { get; set; }

        public string LinkMainComName { get; set; }

        public Guid LinkMainCID { get; set; }
    }

    [NotMapped]
    public class BrandByMainComPageList
    {
        public int TotalRecord { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<BrandByMainCom> Rows { get; set; }
    }
}