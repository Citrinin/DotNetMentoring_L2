using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProfileSample.DAL;
using ProfileSample.Models;

namespace ProfileSample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var context = new DAL.ProfileSample();

            var sources = context.ImgSources.Take(20).Select(x => x.Id);
            
            var model = new List<ImageModel>();

            foreach (var id in sources)
            {
                var item = context.ImgSources.Find(id);

                var obj = new ImageModel()
                {
                    Name = item.Name,
                    Data = item.Data
                };

                model.Add(obj);
            } 

            return View(model);
        }


        public async Task<ActionResult> IndexKate()
        {
            var context = new DAL.ProfileSample();

            var sources = context.ImgSources.Take(20).Select(x => x.Id);

            var images = await context.ImgSources.Where(img => sources.Contains(img.Id)).ToListAsync();

            var model = new List<ImageModel>( images.AsParallel().Select(img => new ImageModel()
            {
                Name = img.Name,
                Data = CreateThumbnail(img.Data, 300, 150)
            }));

            return View(model);
        }


        public static byte[] CreateThumbnail(byte[] image, int width, int height)
        {
            using (MemoryStream originalImgMs = new MemoryStream(image), resultImgMs = new MemoryStream())
            {
                var fullSizeImage = Image.FromStream(originalImgMs);

                var newImage = fullSizeImage.GetThumbnailImage(width, height, null, IntPtr.Zero);

                newImage.Save(resultImgMs, System.Drawing.Imaging.ImageFormat.Png);
                return resultImgMs.ToArray();
            }
        }

        public ActionResult Convert()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Content/Img"), "*.jpg");

            using (var context = new ProfileSampleEntities())
            {
                foreach (var file in files)
                {
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        byte[] buff = new byte[stream.Length];

                        stream.Read(buff, 0, (int) stream.Length);

                        var entity = new ImgSource()
                        {
                            Name = Path.GetFileName(file),
                            Data = buff,
                        };

                        context.ImgSources.Add(entity);
                        context.SaveChanges();
                    }
                } 
            }

            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}