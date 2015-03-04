using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;

namespace Lab_1
{
    public partial class Form1 : Form
    {
        enum InitState { None, Code, Token };
        InitState selfstate = InitState.None;

        string client_id = "ot1elmv9hghhz8i";
        string client_secret = "ngt3y2kqxpc4pfm";
        string redirectUrl = "http://localhost";
        string code;
        string access_token;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {   SetState(InitState.None);
        }

        void SetState(InitState newstate)
        {   switch (selfstate = newstate)
            {   case InitState.None:
                    SendAuthRequest();
                    break;
                case InitState.Code:
                    SendTokenRequest();
                    break;
                case InitState.Token:
                    DoSomethingWithToken();
                    break;
            }
        }

        private void SendAuthRequest()
        {   webControl1.Source = new Uri("https://www.dropbox.com/1/oauth2/authorize"
                                        + "?response_type=" + "code"
                                        + "&client_id=" + client_id
                                        + "&redirect_uri=" + redirectUrl
            );
        }

        private void SendTokenRequest()
        {   var rc = new RestClient("https://api.dropbox.com/1/oauth2/token");
            var rq = new RestRequest(Method.POST);
            rq.AddParameter("code", code);
            rq.AddParameter("grant_type", "authorization_code");
            rq.AddParameter("client_id", client_id);
            rq.AddParameter("client_secret", client_secret);
            rq.AddParameter("redirect_uri", redirectUrl);

            var response_raw = rc.Execute(rq).Content;
            var response = response_raw
                .Replace("\"", "")
                .Split(new char[] {',', '{', '}', ':', ' '},
                StringSplitOptions.RemoveEmptyEntries);

            if (response[0] == "error")
            {   MessageBox.Show("Не получен access-токен");
                SetState(InitState.None);
            }
            else
            {   access_token = response[1];
                SetState(InitState.Token);
            }
        }

        private void DoSomethingWithToken()
        {   var rc = new RestClient("https://api.dropbox.com/1/account/info");
            var rq = new RestRequest(Method.GET);
            rq.AddParameter("access_token", access_token);

            var response_raw = rc.Execute(rq).Content;
            var response = response_raw.Replace(", ", "\r\n");
            response = response.Replace("\"", "");
            response = response.Replace("{", "");
            response = response.Replace("}", "");

            textBox1.Text = "Информация об аккаунте:\r\n" + response;
        }

        private void Awesomium_Windows_Forms_WebControl_TargetURLChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {
            #region receiving code
            if (webControl1.Source.AbsoluteUri.StartsWith(redirectUrl) && selfstate == InitState.None)
            {
                string s = webControl1.Source.AbsoluteUri.Split(new string[] { "localhost" }, StringSplitOptions.None).ToArray()[1];
                var parvals = s.Split(new char[] { '/', '?', '=', '&' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                if (parvals[0] == "code")
                {   MessageBox.Show("Авторизация прошла успешно");
                    code = parvals[1];
                    SetState(InitState.Code);
                }
                else
                    if (parvals[0] == "error")
                    {   MessageBox.Show("Ошибка авторизации: \r\n" + parvals[1]);
                        SetState(InitState.None);
                    }
                    else
                    {   MessageBox.Show("Что-то пошло не так: \r\n" + webControl1.Source.AbsoluteUri);
                        SetState(InitState.None);
                    }
            }
            #endregion
        }

        private void Awesomium_Windows_Forms_WebControl_CertificateError(object sender, Awesomium.Core.CertificateErrorEventArgs e)
        {   if (e.Url.OriginalString.StartsWith("https://www.dropbox.com"))
            {   e.Handled = Awesomium.Core.EventHandling.Modal;
                e.Ignore = true;
            }
        }

    }

}