using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;

namespace TestImagVideoBrowserSideBar
{
    enum ItemTypeEnum
    {
        IMAGE = 0,
        VIDEO = 1,
    }

    //一个条目的描述器
    class ItemDiscriptor
    {
        public ItemTypeEnum itemType { set; get; } //item类型：avi视频， jpg图片
        public string itemPath { set; get; } //item完整路径
        public string itemName { set; get; } //item的名字
        public int index { set; get; }  //item的序号
        public DateTime itemCreationTime { set; get; } //item的创建时间
        public Bitmap thumbnail; //缩略图
        public ItemDiscriptor(string _itemPath)
        {
            itemPath = _itemPath;
            CheckItemType(ref _itemPath);
            SetItemProperty();
            if (itemType == ItemTypeEnum.IMAGE)
            {
                GetThumbnailFromImage();
            }
            else if (itemType == ItemTypeEnum.VIDEO)
            {
                GetThumbnailFromVideo();
            }
        }

        ~ItemDiscriptor()
        {
            if (thumbnail != null)
                thumbnail.Dispose();
        }

        private void SetItemProperty()
        {
            FileInfo fileInfo = new FileInfo(itemPath);
            //itemCreationTime = fileInfo.CreationTime;
            itemCreationTime = fileInfo.LastWriteTime; //这里用lastwritetime更合适，否则复制图片创建时间比lastwrite时间还晚，不符合逻辑
            itemName = fileInfo.Name;
        }

        //根据完整文件路径判断类型
        private void CheckItemType(ref string filePath)
        {
            string fileStrType = filePath.Substring(filePath.LastIndexOf("."));
            if (fileStrType == ".jpg" || fileStrType == ".jpeg")
            {
                itemType = ItemTypeEnum.IMAGE;
            }
            else if (fileStrType == ".avi" || fileStrType == ".mp4")
            {
                itemType = ItemTypeEnum.VIDEO;
            }
        }

        //从图像获取缩略图
        private void GetThumbnailFromImage()
        {
            FileStream fs = File.OpenRead(itemPath); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            Image result = Image.FromStream(fs);
            fs.Close();
            Image myThumbnailImg = result.GetThumbnailImage(128, 72, () => { return false; }, IntPtr.Zero);
            thumbnail = new Bitmap(myThumbnailImg);
            result.Dispose();
            myThumbnailImg.Dispose();
        }

        private void GetThumbnailFromVideo()
        {
            VideoFileReader videoFileReader = new VideoFileReader();
            videoFileReader.Open(itemPath);
            Bitmap videoFrame = videoFileReader.ReadVideoFrame();
            Image myThumbnailImg = videoFrame.GetThumbnailImage(128, 72, () => { return false; }, IntPtr.Zero);
            thumbnail = new Bitmap(myThumbnailImg);
            videoFrame.Dispose();
            myThumbnailImg.Dispose();
            videoFileReader.Dispose();
        }

    }


    public partial class ImageVideoBrowserSideBar : UserControl
    {
        public string dataPath { set; get; } //控件当前需要显示图像和视频的路径
        public ImageList thumbnailImageList; //需要显示的所列图图片列表
        private List<ItemDiscriptor> itemsList; //所有items的列表

        public delegate void DeleteImgNotifyHandler();
        //声明事件,用户双击一个item触发显示这个item的.
        public event DeleteImgNotifyHandler DeleteImgNotify;

        public delegate void DoubleClickOpenItemNotifyHandler();
        //声明双击打开一个Item（图片，视频）
        public event DoubleClickOpenItemNotifyHandler DBClickOpenItemNotify;


        //根据dataPath进行时间降序排序
        public void SortOrderByTimeDescend()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dataPath);
            if (!dirInfo.Exists)
                return;
            //只保留avi和jpg后缀的
            var fileArray = Directory.EnumerateFiles(dataPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".avi") || s.EndsWith(".jpg"));
            foreach (string currentFile in fileArray)
            {
                itemsList.Add(new ItemDiscriptor(currentFile)); //读取一个新的文件到list中
            }
            //把item根据时间降序排列
            itemsList.Sort((x, y) => y.itemCreationTime.CompareTo(x.itemCreationTime));
        }

        //根据日期对于所有list中的照片进行分组显示, 例如 2020-9-10 的所有照片会在一个组内
        public void GroupItemByDate()
        {
            ///////1. 在数据逻辑端更新数据////////
            //key是日期字符串, value是这个日期对应的list
            Dictionary<string, List<ItemDiscriptor>> groupDateDictionary = new Dictionary<string, List<ItemDiscriptor>>();
            //此时默认list是降序排列的,遍历一次list即可完成所有的日期的分组
            DateTime traverseDt = itemsList.First().itemCreationTime; //把每一个日期和这个进行比较,不同就新创建一个日期
            int itemIdx = 0;
            foreach (var item in itemsList)
            {
                item.index = itemIdx++;
                thumbnailImageList.Images.Add(item.thumbnail);
                TimeSpan ts = traverseDt.Subtract(item.itemCreationTime);
                if (0 != ts.Days) //如果两个DateTime的日期天数不一样,则就要更新数据
                {
                    traverseDt = item.itemCreationTime;
                }
                string dateKey = item.itemCreationTime.ToString("d");
                //还要判断字典中是否有这个value了
                if (groupDateDictionary.ContainsKey(dateKey)) //有则添加,没有则创建
                {
                    groupDateDictionary[dateKey].Add(item);
                }
                else
                {
                    List<ItemDiscriptor> newDate = new List<ItemDiscriptor>();
                    newDate.Add(item);
                    groupDateDictionary[dateKey] = newDate;
                }
            }

            /////////2.在ListView的显示端进行更新数据//////////
            listView_showItems.Items.Clear();
            listView_showItems.BeginUpdate();
            foreach (var oneList in groupDateDictionary)
            {
                //创建这个日期的 分组 listviewGroup
                ListViewGroup curDateLVG = new ListViewGroup(); //创建当前日期的listviewgroup分组
                curDateLVG.Header = oneList.Key;  //设置组的标题为当前日期。
                curDateLVG.Name = oneList.Key;
                curDateLVG.HeaderAlignment = HorizontalAlignment.Left;   //设置组标题文本的对齐方式。（默认为Left）
                listView_showItems.Groups.Add(curDateLVG);   //把当前日期分组添加到listview中 

                //然后遍历这个日期的List,对于每一个item创建listviewitem
                foreach (var item in oneList.Value)
                {
                    ListViewItem oneListViewItem = new ListViewItem();
                    oneListViewItem.ImageIndex = item.index;
                    oneListViewItem.Text = item.itemName;
                    listView_showItems.Groups[oneList.Key].Items.Add(oneListViewItem);
                    listView_showItems.Items.Add(oneListViewItem);
                }
            }

            listView_showItems.LargeImageList = thumbnailImageList;
            listView_showItems.EndUpdate();
        }



        public ImageVideoBrowserSideBar()
        {
            InitializeComponent();
            thumbnailImageList = new ImageList(); //创建显示的集合
            thumbnailImageList.ImageSize = new Size(128, 128); //设置imagelist的 属性
            thumbnailImageList.ColorDepth = ColorDepth.Depth24Bit;

            itemsList = new List<ItemDiscriptor>();  //创建图片,视频items的集合 

            //记得要设置ShowGroups属性为true（默认是false），否则显示不出分组 
            listView_showItems.ShowGroups = true;
        }

        //在listview上单击显示右键菜单
        private void listView_showItems_MouseClick(object sender, MouseEventArgs e)
        {
            listView_showItems.MultiSelect = false;
            if (e.Button == MouseButtons.Right)
            {
                Point p = new Point(e.X, e.Y);
                //contextMenuStrip_openImg.Show(listView_showItems, p);
            }
        }
    }
}


