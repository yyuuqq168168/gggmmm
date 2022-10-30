using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;
using WTelegram;

namespace WTelegramClientTestWF
{
	public partial class MainForm : Form
	{
		private WTelegram.Client _client;

		public MainForm()
		{
			InitializeComponent();
			WTelegram.Helpers.Log = (l, s) => Debug.WriteLine(s);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			_client?.Dispose();
			Properties.Settings.Default.Save();
		}

		private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(((LinkLabel)sender).Tag as string);
		}

		/// <summary>
		/// Login
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void buttonLogin_Click(object sender, EventArgs e)
		{
			buttonLogin.Enabled = false;
			listBox.Items.Add($"Connecting & login into Telegram servers...");
			_client = new WTelegram.Client(int.Parse(textBoxApiID.Text), textBoxApiHash.Text);
			await DoLogin(textBoxPhone.Text);
		}

		private async Task DoLogin(string loginInfo)
		{
			string what = await _client.Login(loginInfo);  // _client.SendMessageAsync()
			if (what != null)
			{
				listBox.Items.Add($"A {what} is required...");
				labelCode.Text = what + ':';
				textBoxCode.Text = "";
				labelCode.Visible = textBoxCode.Visible = buttonSendCode.Visible = true;
				textBoxCode.Focus();
				return;
			}
			panelActions.Visible = true;
			listBox.Items.Add($"We are now connected as {_client.User}");
		}

		/// <summary>
		/// 验证码
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void buttonSendCode_Click(object sender, EventArgs e)
		{
			labelCode.Visible = textBoxCode.Visible = buttonSendCode.Visible = false;
			await DoLogin(textBoxCode.Text);
		}

		private async void buttonGetChats_Click(object sender, EventArgs e)
		{
			if (_client.User == null)
			{
				MessageBox.Show("You must login first.");
				return;
			}
			var chats = await _client.Messages_GetAllChats(null);
			listBox.Items.Clear();
			foreach (var chat in chats.chats.Values)
				if (chat.IsActive)
					listBox.Items.Add(chat);
		}

        /// <summary>
        /// GetMembers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonGetMembers_Click(object sender, EventArgs e)
		{
			if (listBox.SelectedItem is not ChatBase chat)
			{
				MessageBox.Show("You must select a chat in the list first");
				return;
			}
			var users = chat is Channel channel
				? (await _client.Channels_GetAllParticipants(channel)).users
				: (await _client.Messages_GetFullChat(chat.ID)).users;
			listBox.Items.Clear();
			foreach (var user in users.Values)
				listBox.Items.Add(user);
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void buttonSendMsg_Click(object sender, EventArgs e)
		{
			var msg = Interaction.InputBox("Type some text to send to ourselves\n(Saved Messages)", "Send to self");
			if (!string.IsNullOrEmpty(msg))
			{
				msg = "_Here is your *saved message*:_\n" + Markdown.Escape(msg);
				var entities = _client.MarkdownToEntities(ref msg);
				//var msgInfo = await _client.SendMessageAsync(InputPeer.Self, msg, entities: entities);

				var self = InputPeer.Self;
                // var msgInfo = await _client.SendMessageAsync(self, msg, entities: entities);

                // var chats = await _client.Messages_GetAllChats();

                var chats = await _client.Messages_GetAllChats(null);

                InputPeer peer = chats.chats[1847008776]; // the chat we want
                DateTime when = DateTime.UtcNow.AddMinutes(3);

				//await _client.SendMessageAsync(peer, "This will be posted in 3 minutes", schedule_date: when);

				try
				{
					await _client.SendMessageAsync(peer, "测试信息");

					//await _client.SendMessageAsync(peer, "1aaa300");
					//await _client.SendMessageAsync(peer, "1aaa200");
					//await _client.SendMessageAsync(peer, "1aaa500");

					//while (true)
					//{
     //                   string randNum = 'a' + new Random().Next(5_000).ToString();

     //                   await _client.SendMessageAsync(peer, randNum);

					//	System.Threading.Thread.Sleep(3000);
     //               }
                }
				catch (Exception ex)
				{
					throw ex;
				}
                
            }
		}
	}
}
