using IqdbApi;
using IqdbApi.Models;
using System.Text;

namespace IqdbManualTests
{
    public partial class FrmManualTests : Form
    {
        private bool _searching = false;
        public FrmManualTests()
        {
            InitializeComponent();
        }

        private void SearchingFlash()
        {
            _searching = true;
            int dotCount = -1;
            lblSearching.Visible = true;
            Task.Run(() =>
            {
                while (_searching)
                {
                    Task.Delay(200).Wait();
                    dotCount++;
                    Invoke(new Action(() => 
                    {
                        if (dotCount == 0)
                            lblSearching.Text = "Searching.";
                        else if (dotCount == 1)
                            lblSearching.Text = "Searching..";
                        else if (dotCount == 2)
                            lblSearching.Text = "Searching...";
                        else
                        {
                            lblSearching.Text = "Searching";
                            dotCount = -1;
                        }
                    }));
                }
                Invoke(new Action(() => lblSearching.Visible = false));
            });
        }

        private async void btnSearchByUrl_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txbUrl.Text))
            {
                SearchingFlash();
                try
                {
                    IIqdbClient api = new IqdbClient(null, 5100, chkHttps.Checked);
                    SearchResult searchResults = await api.SearchUrl(txbUrl.Text);
                    FillResults(searchResults, false);
                }
                catch (Exception ex)
                {
                    txbSearchResults.AppendText(ex.Message + "\r\n");
                }
                _searching = false;
            }
        }

        private async void btnSearchByFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(txbFileName.Text))
            {
                using (var fs = new FileStream(txbFileName.Text, FileMode.Open))
                {
                    SearchingFlash();
                    try
                    {
                        IIqdbClient api = new IqdbClient(null, 5100, chkHttps.Checked);
                        SearchResult searchResults = await api.SearchFile(fs);
                        FillResults(searchResults, false);
                    }
                    catch (Exception ex)
                    {
                        txbSearchResults.AppendText(ex.Message + "\r\n");
                    }
                    _searching = false;
                }
            }
        }

        private async void btnSearchByUrl_3d_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txbUrl.Text))
            {
                SearchingFlash();
                try
                {
                    IIqdbClient api = new Iqdb3dClient(null, 5100, chkHttps.Checked);
                    SearchResult searchResults = await api.SearchUrl(txbUrl.Text);
                    FillResults(searchResults, true);
                }
                catch (Exception ex)
                {
                    txbSearchResults.AppendText(ex.Message + "\r\n");
                }
                _searching = false;
            }
        }

        private async void btnSearchByFile_3d_Click(object sender, EventArgs e)
        {
            if (File.Exists(txbFileName.Text))
            {
                using (var fs = new FileStream(txbFileName.Text, FileMode.Open))
                {
                    SearchingFlash();
                    try
                    {
                        IIqdbClient api = new Iqdb3dClient(null, 5100, chkHttps.Checked);
                        SearchResult searchResults = await api.SearchFile(fs);
                        FillResults(searchResults, true);
                    }
                    catch (Exception ex)
                    {
                        txbSearchResults.AppendText(ex.Message + "\r\n");
                    }
                    _searching = false;
                }
            }
        }

        private async void FillResults(SearchResult results, bool is3D)
        {
            if (results != null && results.Matches.Count > 0)
            {
                var firstItem = results.Matches.OrderByDescending(m => m.Similarity).First();
                string thuImgUrl = $"https://{(is3D ? "3d" : "www")}.iqdb.org{firstItem.PreviewUrl}";
                var downloading = DownloadImage(thuImgUrl);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Url£º" + firstItem.Url);
                stringBuilder.AppendLine($"Similarity£º{firstItem.Similarity}%({ firstItem.Source.ToString().Replace("_", "") })");
                stringBuilder.AppendLine("Rating£º" + firstItem.Rating);
                stringBuilder.AppendLine("Tags£º" + string.Join(',', firstItem.Tags));
                txbSearchResults.Text = stringBuilder.ToString();
                picResult.Image = await downloading;
            }
        }

        public static async Task<Bitmap> DownloadImage(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                if (bytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        Bitmap bmp = new Bitmap(Image.FromStream(ms));
                        return bmp;
                    }
                }
                return null;
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image File|*.bmp;*.dib;*.jpg;*.jpe;*.jpeg;*.jfif;*.png;*.tif;*.tiff;*.gif;*.heic;";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                txbFileName.Text = openFileDialog.FileName;
        }
    }
}