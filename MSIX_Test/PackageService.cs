using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace MSIX_Test
{
    public class PackageService
    {
        private RestClient _client { get; set; }
        private PackageManager _packageManager = new PackageManager();
        /// <summary>
        /// 現在インストールされているパッケージ
        /// </summary>
        private Package? _package { get; set; }
        /// <summary>
        /// Package.appxmanifestファイルのパッケージ化タブのパッケージ名
        /// </summary>
        private string _packageName { get; set; }
        /// <summary>
        /// インストール完了イベント
        /// </summary>
        public AsyncOperationWithProgressCompletedHandler<DeploymentResult, DeploymentProgress> InstallResultHandler { get; set; }
        /// <summary>
        /// インストールの進捗イベント
        /// </summary>
        public AsyncOperationProgressHandler<DeploymentResult, DeploymentProgress> InstallProgressHandler { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName">Package.appxmanifestファイルのパッケージ化タブのパッケージ名</param>
        public PackageService(string packageName)
        {
            _client = new RestClient();
            _packageName = packageName;
            _package = GetPackage();
        }

        /// <summary>
        /// PCにインストールされているパッケージを取得
        /// </summary>
        /// <returns></returns>
        private Package? GetPackage()
        {
            Package package = null;
            try
            {
                // パッケージの検索
                // 見つからない場合にExceptionになるのでTry
                var packages = _packageManager.FindPackagesForUser(string.Empty);
                var packageFullName = packages.FirstOrDefault(x => x.Id.Name == _packageName)?.Id.FullName ?? "";
                package = _packageManager.FindPackageForUser(string.Empty, packageFullName);
            }
            catch (Exception e)
            {
            }
            return package;
        }

        /// <summary>
        /// 指定バージョンのパッケージをサーバーからダウンロード
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private async Task DownloadPackageAsync(string version)
        {   
            var stream  = await _client.GetStream($"https://localhost:44346/MSIX", new { version });
            using (var fileStream = new FileStream(GetSaveFilePath(), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// msixファイルがダウンロードされるパスを取得
        /// </summary>
        /// <returns></returns>
        private string GetSaveFilePath()
        {
            var localDir = "";
            try
            {
                // UWPアプリ
                localDir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            catch (Exception ex)
            {
                // UWPアプリでない
                localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            string saveDirectory = Path.Combine(localDir, "MSIXPackages");

            Directory.CreateDirectory(saveDirectory);

            return Path.Combine(saveDirectory, _packageName);
        }

        /// <summary>
        /// ダウンロードしたパッケージのインストール
        /// </summary>
        /// <returns></returns>
        private async Task InstallAsync()
        {
            var uri = new Uri(GetSaveFilePath());
            PackageManager packagemanager = new PackageManager();

            var asyncProgress = packagemanager.AddPackageAsync(
                uri,
                null,
                DeploymentOptions.ForceApplicationShutdown
            );

            if (InstallResultHandler != null)
            {
                asyncProgress.Completed += InstallResultHandler;
            }

            if (InstallProgressHandler != null)
            {
                asyncProgress.Progress += InstallProgressHandler;
            }

            // ただasyncProgressをawaitするとフリーズすることがあるのでTaskRun&whileで待つ
            await Task.Run(() =>
            {
                while (asyncProgress.Status == AsyncStatus.Started)
                {
                }
            });

        }

        /// <summary>
        ///  パッケージのダウンロードとインストール
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private async Task DownlogdAndInstall(string version)
        {
            await DownloadPackageAsync(version);
            await InstallAsync();
        }

        /// <summary>
        /// 現在インストールされているパッケージのバージョンと
        /// サーバーにあるバージョンを比較して、必要の場合はダウンロードとインストールを行う
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePackageAsync()
        {
            var newVersion = GetNewVersion();
            if (_package == null)
            {
                // なければ新規インストール
                await DownlogdAndInstall(newVersion.ToString());
                return;
            }

            var currentVersion = new Version(GetPackageVersionText());
            if (newVersion.CompareTo(currentVersion) > 0)
            {
                await DownlogdAndInstall(newVersion.ToString());
            }
        }

        /// <summary>
        /// サーバーで指定されているパッケージのバージョン取得
        /// </summary>
        /// <returns></returns>
        private Version GetNewVersion()
        {
            // 今回はテストのため必ず、アップデートが走るように発行するパッケージより高くしておきます
            // 実際にはサーバー等から取得しましょう
            return new Version("2.0.0.0");
        }

        /// <summary>
        /// 現在インストールされているパッケージのバージョンを取得
        /// </summary>
        /// <returns></returns>
        public string GetPackageVersionText()
        {
            if (_package == null)
                return "";

            var v = _package.Id.Version;
            return string.Format("{0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);
        }

    }
}
