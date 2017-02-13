﻿using System.Collections.ObjectModel;
using System.Net.Http;
using GithubXamarin.Core.Contracts.Service;
using GithubXamarin.Core.Contracts.ViewModel;
using GithubXamarin.Core.Messages;
using MvvmCross.Plugins.Messenger;
using Octokit;

namespace GithubXamarin.Core.ViewModels
{
    public class NotificationsViewModel : BaseViewModel, INotificationsViewModel
    {
        #region Properties and Commands

        private readonly INotificationDataService _notificationDataService;

        private ObservableCollection<Notification> _notifications;
        public ObservableCollection<Notification> Notifications
        {
            get { return _notifications; }
            set
            {
                _notifications = value;
                RaisePropertyChanged(() => Notifications);
            }
        }

        #endregion


        public NotificationsViewModel(IGithubClientService githubClientService, INotificationDataService notificationDataService, IMvxMessenger messenger, IDialogService dialogService) : base(githubClientService, messenger, dialogService)
        {
            _notificationDataService = notificationDataService;
        }

        public async void Init(long? repositoryId = null)
        {
            if (!IsInternetAvailable())
            {
                await DialogService.ShowDialogASync("What is better ? To be born good or to overcome your evil nature through great effort ?", "No Internet Connection!");
                return;
            }
            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = true });
            try
            {
                if (repositoryId.HasValue)
                {
                    Messenger.Publish(new AppBarHeaderChangeMessage(this) {HeaderTitle = "Notifications"});
                    Notifications = await _notificationDataService.GetAllNotificationsForRepository(repositoryId.Value,
                        GithubClientService.GetAuthorizedGithubClient());
                }
                else
                {
                    Messenger.Publish(new AppBarHeaderChangeMessage(this) {HeaderTitle = "Your Notifications"});
                    Notifications =
                        await _notificationDataService.GetAllNotificationsForCurrentUser(
                            GithubClientService.GetAuthorizedGithubClient());
                }
            }
            catch (HttpRequestException)
            {
                await DialogService.ShowDialogASync("The internet seems to be working but the code threw an HttpRequestException. Try again.", "Hmm, this is weird!");
            }
            Messenger.Publish(new LoadingStatusMessage(this) { IsLoadingIndicatorActive = false });
        }
    }
}
