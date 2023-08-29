using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Dictionary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class ModLoader
{
    public string Name { get; }
    public string Author { get; }

    /// <summary>
    /// 如果是上传至Steam,则为SteamUserID
    /// </summary>
    public long AuthorID { get; }

    /// <summary>
    /// 上传至Steam的ItemID
    /// </summary>
    public ulong ItemID { get; }
    public string Intro { get; }
    public DirectoryInfo Path { get; }
    public int GameVer { get; }
    public int Ver { get; }
    public HashSet<string> Tag { get; } = new();
    public bool SuccessLoad { get; } = true;
    public DateTime CacheDate { get; } = DateTime.MinValue;
    public List<PetLoader> Pets { get; } = new();
    public List<Food> Foods { get; } = new();
    public List<LowText> LowTexts { get; } = new();
    public Dictionary<string, I18nModInfoModel> I18nDatas { get; } = new();

    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();
    public List<ClickText> ClickTexts { get; } = new();

    public ModLoader(DirectoryInfo directory)
    {
        try
        {
            Path = directory;
            LpsDocument modlps = new LpsDocument(
                File.ReadAllText(directory.FullName + @"\info.lps")
            );
            Name = modlps.FindLine("vupmod").Info;
            Intro = modlps.FindLine("intro").Info;
            GameVer = modlps.FindSub("gamever").InfoToInt;
            Ver = modlps.FindSub("ver").InfoToInt;
            Author = modlps.FindSub("author").Info.Split('[').First();
            if (modlps.FindLine("authorid") != null)
                AuthorID = modlps.FindLine("authorid").InfoToInt64;
            else
                AuthorID = 0;
            if (modlps.FindLine("itemid") != null)
                ItemID = Convert.ToUInt64(modlps.FindLine("itemid").info);
            else
                ItemID = 0;
            CacheDate = modlps.GetDateTime("cachedate", DateTime.MinValue);

            //MOD未加载时支持翻译
            foreach (var line in modlps.FindAllLine("lang"))
            {
                var i18nData = new I18nModInfoModel();
                foreach (var sub in line)
                {
                    if (sub.Name == Name)
                        i18nData.Name.Value = sub.Info;
                    else if (sub.Name == Intro)
                        i18nData.Description.Value = sub.Info;
                }
                I18nDatas.Add(line.Info, i18nData);
            }
            DirectoryInfo? langDirectory = null;
            foreach (DirectoryInfo di in Path.EnumerateDirectories())
            {
                switch (di.Name.ToLower())
                {
                    case "pet":
                        //宠物模型
                        Tag.Add("pet");
                        foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                        {
                            var lps = new LpsDocument(File.ReadAllText(fi.FullName));
                            if (lps.First().Name.ToLower() == "pet")
                            {
                                var name = lps.First().Info;
                                Pets.Add(new PetLoader(lps, di));
                                //var p = mw.Pets.FirstOrDefault(x => x.Name == name);
                                //if (p == null)
                                //    mw.Pets.Add(new PetLoader(lps, di));
                                //else
                                //{
                                //    p.path.Add(di.FullName + "\\" + lps.First()["path"].Info);
                                //    p.Config.Set(lps);
                                //}
                            }
                        }
                        break;
                    case "food":
                        Tag.Add("food");
                        foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                        {
                            var tmp = new LpsDocument(File.ReadAllText(fi.FullName));
                            foreach (ILine li in tmp)
                            {
                                var food = LPSConvert.DeserializeObject<Food>(li);
                                var imagePath = $"{Path.FullName}\\image\\food\\{food.Name}.png";
                                if (File.Exists(imagePath))
                                    food.Image = imagePath;
                                Foods.Add(food);
                                //string tmps = li.Find("name").info;
                                //mw.Foods.RemoveAll(x => x.Name == tmps);
                                //mw.Foods.Add(LPSConvert.DeserializeObject<Food>(li));
                            }
                        }
                        break;
                    case "image":
                        Tag.Add("image");
                        break;
                    case "text":
                        Tag.Add("text");
                        foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                        {
                            var tmp = new LpsDocument(File.ReadAllText(fi.FullName));
                            foreach (ILine li in tmp)
                            {
                                switch (li.Name.ToLower())
                                {
                                    case "lowfoodtext":
                                        LowTexts.Add(LPSConvert.DeserializeObject<LowText>(li));
                                        break;
                                    case "lowdrinktext":
                                        LowTexts.Add(LPSConvert.DeserializeObject<LowText>(li));
                                        break;
                                    case "clicktext":
                                        ClickTexts.Add(LPSConvert.DeserializeObject<ClickText>(li));
                                        break;
                                    //case "selecttext":
                                    //    mw.SelectTexts.Add(
                                    //        LPSConvert.DeserializeObject<SelectText>(li)
                                    //    );
                                    //    break;
                                }
                            }
                        }
                        break;
                    case "lang":
                        Tag.Add("lang");
                        langDirectory = di;
                        //foreach (FileInfo fi in di.EnumerateFiles("*.lps"))
                        //{
                        //    //LocalizeCore.AddCulture(
                        //    //    fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length),
                        //    //    new LPS_D(File.ReadAllText(fi.FullName))
                        //    //);
                        //}
                        //foreach (DirectoryInfo dis in di.EnumerateDirectories())
                        //{
                        //    foreach (FileInfo fi in dis.EnumerateFiles("*.lps"))
                        //    {
                        //        //LocalizeCore.AddCulture(
                        //        //    dis.Name,
                        //        //    new LPS_D(File.ReadAllText(fi.FullName))
                        //        //);
                        //    }
                        //}

                        //if (mw.Set.Language == "null")
                        //{
                        //    LocalizeCore.LoadDefaultCulture();
                        //}
                        //else
                        //    LocalizeCore.LoadCulture(mw.Set.Language);
                        break;
                }
            }
            if (langDirectory is null)
                return;
            foreach (DirectoryInfo dis in langDirectory.EnumerateDirectories())
            {
                OtherI18nDatas.Add(dis.Name, new());
                foreach (FileInfo fi in dis.EnumerateFiles("*.lps"))
                {
                    var lps = new LPS(File.ReadAllText(fi.FullName));
                    foreach (var item in lps)
                    {
                        if (OtherI18nDatas[dis.Name].ContainsKey(item.Name) is false)
                            OtherI18nDatas[dis.Name].Add(item.Name, item.Info);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Tag.Add("该模组已损坏");
            SuccessLoad = false;
        }
    }

    public void WriteFile()
    {
        var lps = new LpsDocument(File.ReadAllText(Path.FullName + @"\info.lps"));
        lps.FindLine("vupmod").Info = Name;
        lps.FindLine("intro").Info = Intro;
        lps.FindSub("gamever").InfoToInt = GameVer;
        lps.FindSub("ver").InfoToInt = Ver;
        lps.FindSub("author").Info = Author;
        lps.FindorAddLine("authorid").InfoToInt64 = AuthorID;
        lps.FindorAddLine("itemid").info = ItemID.ToString();
        File.WriteAllText(Path.FullName + @"\info.lps", lps.ToString());
    }
}
