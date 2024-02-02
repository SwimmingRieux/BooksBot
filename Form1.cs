using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using Telegram.Bot.Types.InputFiles;

namespace MyBot
{
    public partial class Form1 : Form
    {

        private static string Token = "";
        private Thread botThread;
        private Telegram.Bot.TelegramBotClient bot;
        private ReplyKeyboardMarkup mainKM;
        List<string> IDs = new List<string>();
        string path = System.IO.Directory.GetCurrentDirectory().Replace("Debug","1.html");
        IWebDriver driver = new ChromeDriver();
        

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            driver.Navigate().GoToUrl(path);
            var IDss = driver.FindElements(By.TagName("p"));
            
            foreach(var idd in IDss)
            {
                IDs.Add(idd.Text);
            }
            Token = txtToken.Text;
            botThread = new Thread(new ThreadStart(runBot));
            botThread.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainKM = new ReplyKeyboardMarkup();

            List<KeyboardButton> row1 = new List<KeyboardButton>();
            List<KeyboardButton> row2 = new List<KeyboardButton>();
            row1.Add(new KeyboardButton("جست و جوی کتاب" + "📚"));
            row1.Add(new KeyboardButton("جست و جوی نویسنده" + "🙎‍♀️"));
            row1.Add(new KeyboardButton("جست و جوی ژانر" + "🖊️"));
            row2.Add(new KeyboardButton("ارتباط با مدیر" + "\U0000260E"));
            row2.Add(new KeyboardButton("درباره ما" + "\U0001F4E2"));
            var buttons = new List<List<KeyboardButton>>();
            buttons.Add(row2);
            buttons.Add(row1);
            mainKM.Keyboard = buttons;
        }


        void runBot()
        {
            bot = new Telegram.Bot.TelegramBotClient(Token);

            this.Invoke(new Action(() =>
            {
                lblStatus.Text = "Online";
                lblStatus.ForeColor = Color.Green;
            }));
            int offset = 0;

            while (true)
            {
                Telegram.Bot.Types.Update[] update = bot.GetUpdatesAsync(offset).Result;

                foreach (var up in update)
                {
                    offset = up.Id + 1;

                    if (up.Message == null)
                        continue;

                    
                    
                    var text = up.Message.Text.ToLower();
                    var from = up.Message.From;
                    var chatId = up.Message.Chat.Id;
                    
                        if (text.Contains("/start"))
                        {
                            StringBuilder sb = new StringBuilder();

                            sb.AppendLine("سلام" + up.Message.From.Username);
                            sb.AppendLine("به خانه کتاب خوش آمدید");
                            sb.AppendLine("هر چه بخواهید اینجا هست");

                            bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, mainKM);
                            if (!IDs.Contains(chatId.ToString()))
                            {
                                IDs.Add(chatId.ToString());
                                File.AppendAllText(path, "<p>"+chatId+"</p>");
                            }
                        }
                        else if (text.Contains("درباره ما"))
                        {

                        }
                        else if (text.Contains("جست و جوی کتاب"))
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("برای جست و جوی یک کتاب،مثلا اگر نام کتاب شما،فلان است باید این عبارت را وارد کنید : کتاب فلان");
                            bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, mainKM);
                        }
                        else if (text.Contains("جست و جوی ژانر"))
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("برای جست و جوی کتاب های یک ژانر خاص،مثلا اگر نام ژانر مورد نظر شما،رمان است باید این عبارت را وارد کنید : ژانر رمان");
                            bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, mainKM);
                        }
                        else if (text.Contains("جست و جوی نویسنده"))
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("برای جست و جوی کتاب های یک نویسنده خاص،مثلا اگر نام نویسنده مورد نظر شما،فلان است باید این عبارت را وارد کنید : نویسنده فلان);
                            bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, mainKM);
                        }
                        else if (text.Contains("ارتباط با مدیر"))
                        {
                        }
                        else
                        {
                            if (text.StartsWith("ژانر"))
                            {
                                text = text.Replace("ژانر ", "%23");
                                text = text.Replace(" ", "_");
                            }
                            else if (text.StartsWith("نویسنده"))
                            {
                                text = text.Replace("نویسنده ", "%23");
                                text = text.Replace(" ", "_");
                            }
                            else if (text.StartsWith("کتاب"))
                            {
                                text = text.Remove(0, 5);
                            }
                            try
                            {
                                bot.SendTextMessageAsync(chatId, "Please wait a second,Bot is searching...", ParseMode.Html, false, false, 0, mainKM);
                                driver.Navigate().GoToUrl("https://telegram.me/s/ketabarman?q=" + text);
                                var res = driver.FindElements(By.CssSelector("div[class='tgme_widget_message js-widget_message']"));
                                if (res.Count() == 0)
                                {
                                    bot.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: "متاسفانه جست و جوی شما حاصلی در بر نداشت،با توجه به قوانین جست و جو دوباره تلاش کنید یا اگر کتاب شما یافت نشد آن را به ادمین گزارش دهید.",
                                            replyMarkup: mainKM
                                            );
                                }
                                foreach (var itee in res)
                                {
                                    if (!(itee.Text.Contains("#سینما_کتاب") || itee.Text.Contains("#تیکه_کتاب") || itee.Text.Contains("#شخصیت_های_تاثیرگذار") || itee.Text.Contains("#سینما_کتاب") || itee.Text.Contains("#خلاصه_کتابها") || itee.Text.Contains("#پادکست") || itee.Text.Contains("#رادیو_کتاب") || itee.Text.Contains("#کتاب_باز") || itee.Text.Contains("#مستند_کتاب") || itee.Text.Contains("#باهم_بخوانیم") || itee.Text.Contains("#بنویس") || itee.Text.Contains("js-poll")))
                                    {
                                        string sc = itee.GetAttribute("data-post");
                                        sc = sc.Insert(0, "Audio: https://t.me/");
                                        bot.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: sc,
                                            replyMarkup: mainKM
                                            );
                                    }

                                }

                            }
                            catch
                            {
                                bot.SendTextMessageAsync(chatId, "مشکلی پیش آمده لطفا چند دقیقه دیگر دوباره تلاش کنید", ParseMode.Html, false, false, 0, mainKM);
                            }

                        }
                    
                    


                    dgReport.Invoke(new Action(() =>
                    {
                        dgReport.Rows.Add(chatId, from.Username, text, up.Message.MessageId,
                            up.Message.Date.ToString("yyyy/MM/dd - HH:mm"));
                    }));

                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            botThread.Abort();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
                foreach(var iteem in IDs)
                {
                    bot.SendTextMessageAsync(iteem, txtMessage.Text, ParseMode.Html, true);
                }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFile.FileName;
            }
        }

        private void btnPhoto_Click(object sender, EventArgs e)
        {
           
                
                FileStream imageFile = System.IO.File.Open(txtFilePath.Text, FileMode.Open);

                foreach(var iteem in IDs)
                {
                    bot.SendPhotoAsync(iteem, new InputOnlineFile(imageFile, "1234.jpg"), txtMessage.Text);
                }
                
            
        }

    }
}
