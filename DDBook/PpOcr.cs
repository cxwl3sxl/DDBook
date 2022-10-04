using System;
using System.IO;
using System.Threading.Tasks;
using PaddleOCRSharp;

namespace DDBook
{
    static class PpOcr
    {
        private static readonly PaddleOCREngine Engine;

        static PpOcr()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var config = new OCRModelConfig()
            {
                det_infer = Path.Combine(dir, "en_PP-OCRv3", "en_PP-OCRv3_det_infer"),
                rec_infer = Path.Combine(dir, "en_PP-OCRv3", "en_PP-OCRv3_rec_infer"),
                cls_infer = Path.Combine(dir, "en_PP-OCRv3", "ch_ppocr_mobile_v2.0_cls_infer"),
                keys = Path.Combine(dir, "en_PP-OCRv3", "en_dict.txt"),
            };
            var parameter = new OCRParameter()
            {
                rec_img_h = 48
            };
            Engine = new PaddleOCREngine(config, parameter);
        }

        public static Task<string> Detect(string image)
        {
            return !File.Exists(image)
                ? Task.FromResult("文件不存在")
                : Task.Factory.StartNew(() => Engine.DetectText(image)?.Text ?? "无识别结果");
        }
    }
}
