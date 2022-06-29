using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;
using static Teach_MOTP.Models.HomeModel;

namespace Teach_MOTP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public decimal timeStampEpoch = (decimal)Math.Round((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, 0); //Unix timestamp

        /// <summary>
        /// 產生使用者 QR Code
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public ActionResult GenUserQRCode(GenUserQRCodeIn inModel)
        {
            GenUserQRCodeOut outModel = new GenUserQRCodeOut();
            outModel.ErrMsg = "";
            if (inModel.UserKey.Length != 16)
            {
                outModel.ErrMsg = "密鑰長度需為 16 碼";
            }
            if (inModel.UserPin.Length != 4)
            {
                outModel.ErrMsg = "PIN 長度需為 4 碼";
            }
            int t = 0;
            if (int.TryParse(inModel.UserPin, out t) == false)
            {
                outModel.ErrMsg = "PIN 需為數字";
            }

            if (outModel.ErrMsg == "")
            {
                // 產生註冊資料 For OTP Authenticator
                string motpUser = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SSLOTPAuthenticator><mOTPProfile><ProfileName>{0}</ProfileName><PINType>0</PINType><PINSecurity>0</PINSecurity><Secret>{1}</Secret><AlgorithmMode>0</AlgorithmMode></mOTPProfile></SSLOTPAuthenticator>";
                motpUser = string.Format(motpUser, inModel.UserID, inModel.UserKey);

                // QR Code 設定
                BarcodeWriter bw = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions //設定大小
                    {
                        Height = 300,
                        Width = 300,
                    }
                };
                //產生QRcode
                var img = bw.Write(motpUser); //來源網址
                string FileName = "qrcode.png"; //產生圖檔名稱
                Bitmap myBitmap = new Bitmap(img);
                string FileWebPath = Server.MapPath("~/") + FileName; //完整路徑
                myBitmap.Save(FileWebPath, ImageFormat.Png);
                string FileWebUrl = Url.Content("~/") + FileName; // 產生網頁可看到的路徑
                outModel.FileWebPath = FileWebUrl;
            }

            // 輸出json
            return Json(outModel);
        }

        /// <summary>
        /// 驗證登入
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public ActionResult CheckLogin(CheckLoginIn inModel)
        {
            CheckLoginOut outModel = new CheckLoginOut();
            outModel.ErrMsg = "";
            if (inModel.MOTP == null || inModel.MOTP.Length != 6)
            {
                outModel.ErrMsg = "MOTP 長度需為 6 碼";
            }
            if (inModel.UserKey.Length != 16)
            {
                outModel.ErrMsg = "密鑰長度需為 16 碼";
            }
            if (inModel.UserPin.Length != 4)
            {
                outModel.ErrMsg = "PIN 長度需為 4 碼";
            }
            int t = 0;
            if (int.TryParse(inModel.UserPin, out t) == false)
            {
                outModel.ErrMsg = "PIN 需為數字";
            }

            if (outModel.ErrMsg == "")
            {
                outModel.CheckResult = "登入失敗";

                String otpCheckValueMD5 = "";
                decimal timeWindowInSeconds = 60; //1 分鐘前的 motp 都檢查
                for (decimal i = timeStampEpoch - timeWindowInSeconds; i <= timeStampEpoch + timeWindowInSeconds; i++)
                {
                    otpCheckValueMD5 = (Md5Hash(((i.ToString()).Substring(0, (i.ToString()).Length - 1) + inModel.UserKey + inModel.UserPin))).Substring(0, 6);
                    if (inModel.MOTP.ToLower() == otpCheckValueMD5.ToLower())
                    {
                        outModel.CheckResult = "登入成功";
                        break;
                    }
                }
            }

            // 輸出json
            return Json(outModel);
        }

        /// <summary>
        /// MD5 編碼
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public string Md5Hash(string inputString)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.UTF8.GetBytes(inputString);
                byte[] hash = md5.ComputeHash(input);
                string md5Str = BitConverter.ToString(hash).Replace("-", "");
                return md5Str;
            }
        }
    }
}