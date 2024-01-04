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

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组加载器
/// </summary>
public class ModLoader
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// 如果是上传至Steam,则为SteamUserID
    /// </summary>
    public long AuthorID { get; }

    /// <summary>
    /// 上传至Steam的ItemID
    /// </summary>
    public ulong ItemID { get; }

    /// <summary>
    /// 简介
    /// </summary>
    public string Intro { get; }

    /// <summary>
    /// 模组路径
    /// </summary>
    public DirectoryInfo ModPath { get; }

    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    public int GameVer { get; }

    /// <summary>
    /// 版本
    /// </summary>
    public int Ver { get; }

    /// <summary>
    /// 标签
    /// </summary>
    public HashSet<string> Tag { get; } = new();

    /// <summary>
    /// 缓存数据
    /// </summary>
    public DateTime CacheDate { get; } = DateTime.MinValue;

    /// <summary>
    /// 宠物列表
    /// </summary>
    public List<PetLoader> Pets { get; } = new();

    /// <summary>
    /// 食物列表
    /// </summary>
    public List<Food> Foods { get; } = new();

    /// <summary>
    /// 低状态文本列表
    /// </summary>
    public List<LowText> LowTexts { get; } = new();

    /// <summary>
    /// 点击文本列表
    /// </summary>
    public List<ClickText> ClickTexts { get; } = new();

    /// <summary>
    /// 选择文本列表
    /// </summary>
    public List<SelectText> SelectTexts { get; } = new();

    /// <summary>
    /// I18n数据
    /// </summary>
    public Dictionary<string, I18nModInfoModel> I18nDatas { get; } = new();

    /// <summary>
    /// 其它I18n数据
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();

    public ModLoader(DirectoryInfo path)
    {
        ModPath = path;
        LpsDocument modlps = new LpsDocument(File.ReadAllText(path.FullName + @"\info.lps"));
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
        foreach (DirectoryInfo di in path.EnumerateDirectories())
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
                            var pet = new PetLoader(lps, di);
                            if (pet.Name is null)
                                break;
                            Pets.Add(pet);

                            // ! : 此方法会导致 LoadImageToStream 无法使用
                            //var graphCore = new GraphCore(0);
                            //foreach (var p in pet.path)
                            //    PetLoader.LoadGraph(graphCore, di, p);
                            //MultiGraphs.Add(pet.Name, graphCore);
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
                            if (food.Name is null)
                                break;
                            var imagePath =
                                $"{path.FullName}\\image\\food\\{(string.IsNullOrWhiteSpace(food.Image) ? food.Name : food.Image)}.png";
                            if (File.Exists(imagePath))
                                food.Image = imagePath;
                            Foods.Add(food);
                            //string tmps = li.Find("name").info;
                            //mw.Foods.RemoveAll(x => x.Id == tmps);
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
                                case "selecttext":
                                    SelectTexts.Add(LPSConvert.DeserializeObject<SelectText>(li));
                                    break;
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
                    //    //    fi.Id.Substring(0, fi.Id.Length - fi.Extension.Length),
                    //    //    new LPS_D(File.ReadAllText(fi.FullName))
                    //    //);
                    //}
                    //foreach (DirectoryInfo dis in di.EnumerateDirectories())
                    //{
                    //    foreach (FileInfo fi in dis.EnumerateFiles("*.lps"))
                    //    {
                    //        //LocalizeCore.AddCulture(
                    //        //    dis.Id,
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
                        OtherI18nDatas[dis.Name].TryAdd(item.Name, item.Info);
                }
            }
        }
    }

    //public void WriteFile()
    //{
    //    var lps = new LpsDocument(File.ReadAllText(ModPath.FullName + @"\info.lps"));
    //    lps.FindLine("vupmod").Info = Name;
    //    lps.FindLine("intro").Info = Intro;
    //    lps.FindSub("gamever").InfoToInt = GameVer;
    //    lps.FindSub("ver").InfoToInt = Ver;
    //    lps.FindSub("author").Info = Author;
    //    lps.FindorAddLine("authorid").InfoToInt64 = AuthorID;
    //    lps.FindorAddLine("itemid").info = ItemID.ToString();
    //    File.WriteAllText(ModPath.FullName + @"\info.lps", lps.ToString());
    //}
}
