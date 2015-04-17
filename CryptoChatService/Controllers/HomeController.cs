﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Facebook;
using Newtonsoft.Json;

namespace CryptoChatService.Controllers
{
    public class HomeController : Controller
    {
        public JsonResult Index()
        {
            return GetPublicKeyRSA();
        }

        public JsonResult Connect(string accessToken, string groupId, string ip)
        {
            string path = Server.MapPath("~");

            if (!System.IO.File.Exists(path + "key.txt") || accessToken == null || groupId == null || ip == null)
                return Json("Error", JsonRequestBehavior.AllowGet);

            var bf = new BinaryFormatter();
            using (var stream = System.IO.File.OpenWrite(path + "key.txt"))
            {
                RSAParameters rsaParams = (RSAParameters)bf.Deserialize(stream);
                var rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(rsaParams);
                accessToken = Encoding.Unicode.GetString(rsa.Decrypt(Encoding.Unicode.GetBytes(accessToken), true));
            }

            bool inGroup = false;
            var fb = new FacebookClient(accessToken);
            var result = fb.Get("me") as IDictionary<string, object>;
            var myFbID = result["id"].ToString();

            var response = fb.Get(groupId + "/members") as IDictionary<string, object>;
            var responseData = response["data"].ToString();
            var lis = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(responseData);
            var iddd = lis.FirstOrDefault(x => x["id"].ToString() == myFbID);
            if (iddd != null)
                inGroup = true;

            if (inGroup)
            {
                if (System.IO.File.Exists(path + groupId + ".txt"))
                    using (var stream = System.IO.File.Open(path + groupId + ".txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        stream.Write(Encoding.Unicode.GetBytes(ip), 0, ip.Length);
                    }
                else
                    using (var stream = System.IO.File.Open(path + groupId + ".txt", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        
                        stream.Write(Encoding.Unicode.GetBytes(ip), 0, ip.Length);
                    }
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPublicKeyRSA()
        {
            var bf = new BinaryFormatter();
            var rsa = new RSACryptoServiceProvider();

            string path = Server.MapPath("~");

            if (System.IO.File.Exists(path + "key.txt"))
                using (var stream = System.IO.File.Open(path + "key.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    RSAParameters rsaParam = (RSAParameters)bf.Deserialize(stream);
                    rsa.ImportParameters(rsaParam);
                    return Json(rsa.ExportParameters(false), JsonRequestBehavior.AllowGet);
                }
            else
                using (var stream = System.IO.File.Open(path + "key.txt", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    RSAParameters rsaParam = rsa.ExportParameters(true);
                    bf.Serialize(stream, rsaParam);
                    return Json(rsa.ExportParameters(false), JsonRequestBehavior.AllowGet);
                }
        }
    }
}