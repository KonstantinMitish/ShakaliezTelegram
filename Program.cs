using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Drawing;
using Telegram.Bot.Types.ReplyMarkups;

namespace ShakaliezTelegram
{
  class Program
  {
    static private ImageCodecInfo GetEncoder(ImageFormat format)
    { 
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

      foreach (ImageCodecInfo codec in codecs)
      {
        if (codec.FormatID == format.Guid)
        {
          return codec;
        }
      }
      return null;
    }

    static private void Shakalize(string inName, string outName, int Qual)
    {           
      Bitmap bmp1 = new Bitmap(inName);
      ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                                                
      System.Drawing.Imaging.Encoder myEncoder =
          System.Drawing.Imaging.Encoder.Quality;  

      EncoderParameters myEncoderParameters = new EncoderParameters(1);

      EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Qual);
      myEncoderParameters.Param[0] = myEncoderParameter;
      bmp1.Save(outName, jpgEncoder, myEncoderParameters);

    }

    static async Task Run()
    {
      ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();

      rkm.Keyboard =
          new KeyboardButton[][]
          {
            new KeyboardButton[]
            {
                new KeyboardButton("100")
            },
            
            new KeyboardButton[]
            {
                new KeyboardButton("99"),
                new KeyboardButton("98"),
                new KeyboardButton("97"),
                new KeyboardButton("96"),
                new KeyboardButton("95")
            },

            new KeyboardButton[]
            {
                new KeyboardButton("90"),
                new KeyboardButton("80"),
                new KeyboardButton("70"),
                new KeyboardButton("60"),
                new KeyboardButton("50")
            }, 
            new KeyboardButton[]
            {
                new KeyboardButton("30")
            }
          };
      TelegramBotClient Bot = new TelegramBotClient("******************************");

      Dictionary<long, string> Users = new Dictionary<long, string>();
      var me = await Bot.GetMeAsync();

      Console.WriteLine("Hello my name is {0}", me.Username);

      var offset = 0;

      while (true)
      {
        var updates = await Bot.GetUpdatesAsync(offset);

        foreach (var update in updates)
        {
          if (update.Message.Type == MessageType.TextMessage)
          {
            if (Users.ContainsKey(update.Message.Chat.Id))
            {
              int Value;
              if (int.TryParse(update.Message.Text, out Value))
              {
                if (Value >= 0 && Value <= 100)
                {
                  await Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);
                  string newname = Users[update.Message.Chat.Id] + ".sh_q" + Value.ToString() + ".jpg";
                  Shakalize(Users[update.Message.Chat.Id], newname, 100 - Value);
                  using (var stream = System.IO.File.Open(newname, FileMode.Open))
                  {
                    FileToSend fts = new FileToSend();
                    fts.Content = stream;
                    fts.Filename = newname.Split('\\').Last();
                    await Bot.SendPhotoAsync(update.Message.Chat.Id, fts);
                  }
                }
                else
                {
                  await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Use number 0 - 100", false, false, 0, rkm);
                }                                                                
              }
              else
              {
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Number error", false, false, 0, rkm);
              }
            }
            else
            {
              await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Send me a photo first");
            }
          }

          if (update.Message.Type == MessageType.PhotoMessage)
          {
            var file = await Bot.GetFileAsync(update.Message.Photo.LastOrDefault()?.FileId);
            Console.WriteLine("Received Photo: {0}", file.FilePath);

            if (!Directory.Exists("temp"))
              Directory.CreateDirectory("temp");
            var filename = "temp/" + file.FileId + "." + file.FilePath.Split('.').Last();

            using (var profileImageStream = System.IO.File.Open(filename, FileMode.Create))
            {
              await file.FileStream.CopyToAsync(profileImageStream);
            }
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Now choose shakaling ratio(0 - 100)", false, false, 0, rkm);
            Users[update.Message.Chat.Id] = filename;
          }

          offset = update.Id + 1;
        }
        await Task.Delay(1000);
      }
    }

    static void Main(string[] args)
    {
      try
      {
        Run().Wait();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
