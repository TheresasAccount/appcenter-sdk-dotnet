// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Microsoft.AppCenter.Push;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Contoso.Forms.Demo
{
    using XamarinDevice = Xamarin.Forms.Device;

    [Android.Runtime.Preserve(AllMembers = true)]
    public partial class OthersContentPage
    {
        private const string AccountId = "accountId";

        static bool _eventFilterStarted;

        static OthersContentPage()
        {

        }

        public OthersContentPage()
        {
            InitializeComponent();
            if (XamarinDevice.RuntimePlatform == XamarinDevice.iOS)
            {
                Icon = "handbag.png";
            }

            // Setup auth type dropdown choices
            foreach (var authType in AuthTypeUtils.GetAuthTypeChoiceStrings())
            {
                this.AuthTypePicker.Items.Add(authType);
            }
            this.AuthTypePicker.SelectedIndex = (int)(AuthTypeUtils.GetPersistedAuthType());

            // Setup track update dropdown choices.
            foreach (var trackUpdateType in TrackUpdateUtils.GetUpdateTrackChoiceStrings())
            {
                this.UpdateTrackPicker.Items.Add(trackUpdateType);
            }
            UpdateTrackPicker.SelectedIndex = TrackUpdateUtils.ToPickerUpdateTrackIndex(TrackUpdateUtils.GetPersistedUpdateTrack());

            // Setup when update dropdown choices.
            foreach (var setupTimeType in TrackUpdateUtils.GetUpdateTrackTimeChoiceStrings())
            {
                this.UpdateTrackTimePicker.Items.Add(setupTimeType);
            }
            UpdateTrackTimePicker.SelectedIndex = (int)(TrackUpdateUtils.GetPersistedUpdateTrackTime());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var acEnabled = await AppCenter.IsEnabledAsync();
            RefreshDistributeEnabled(acEnabled);
            RefreshDistributeTrackUpdate();
            RefreshPushEnabled(acEnabled);
            EventFilterEnabledSwitchCell.On = _eventFilterStarted && await EventFilterHolder.Implementation?.IsEnabledAsync();
            EventFilterEnabledSwitchCell.IsEnabled = acEnabled && EventFilterHolder.Implementation != null;
        }

        async void UpdateDistributeEnabled(object sender, ToggledEventArgs e)
        {
            await Distribute.SetEnabledAsync(e.Value);
            var acEnabled = await AppCenter.IsEnabledAsync();
            RefreshDistributeEnabled(acEnabled);
            RefreshDistributeTrackUpdate();
        }

        async void ChangeUpdateTrack(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsFocused" && !this.UpdateTrackPicker.IsFocused)
            {
                var newSelectionCandidate = this.UpdateTrackPicker.SelectedIndex;
                var persistedStartType = TrackUpdateUtils.ToPickerUpdateTrackIndex(TrackUpdateUtils.GetPersistedUpdateTrack());
                if (newSelectionCandidate != persistedStartType)
                {
                    var newTrackUpdateValue = TrackUpdateUtils.FromPickerUpdateTrackIndex(newSelectionCandidate);
                    await TrackUpdateUtils.SetPersistedUpdateTrackAsync(newTrackUpdateValue);
                    if (TrackUpdateUtils.GetPersistedUpdateTrackTime() == UpdateTrackTime.Now)
                    {
                        Distribute.UpdateTrack = (UpdateTrack)newTrackUpdateValue;
                    }
                }
            }
        }

        async void ChangeUpdateTrackTime(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsFocused" && !this.UpdateTrackTimePicker.IsFocused)
            {
                var newSelectionCandidate = this.UpdateTrackTimePicker.SelectedIndex;
                var persistedTimeTrackUpdate = (int)TrackUpdateUtils.GetPersistedUpdateTrackTime();
                if (newSelectionCandidate != persistedTimeTrackUpdate)
                {
                    await TrackUpdateUtils.SetPersistedUpdateTrackTimeAsync((UpdateTrackTime)newSelectionCandidate);
                    if ((UpdateTrackTime)newSelectionCandidate == UpdateTrackTime.BeforeNextStart)
                    {
                        return;
                    }
                    var trackUpdateValue = TrackUpdateUtils.GetPersistedUpdateTrack();
                    Distribute.UpdateTrack = trackUpdateValue;
                    RefreshDistributeTrackUpdate();
                }
            }
        }

        async void UpdatePushEnabled(object sender, ToggledEventArgs e)
        {
            await Push.SetEnabledAsync(e.Value);
            var acEnabled = await AppCenter.IsEnabledAsync();
            RefreshPushEnabled(acEnabled);
        }

        async void RefreshDistributeEnabled(bool _appCenterEnabled)
        {
            DistributeEnabledSwitchCell.On = await Distribute.IsEnabledAsync();
            DistributeEnabledSwitchCell.IsEnabled = _appCenterEnabled;
            RefreshDistributeTrackUpdate();
        }

        async void RefreshDistributeTrackUpdate()
        {
            var isDistributeEnable = await Distribute.IsEnabledAsync();
            if (!isDistributeEnable)
            {
                UpdateTrackPicker.IsEnabled = false;
                UpdateTrackTimePicker.IsEnabled = false;
                return;
            }
            UpdateTrackPicker.IsEnabled = true;
            UpdateTrackPicker.SelectedIndex = TrackUpdateUtils.ToPickerUpdateTrackIndex(TrackUpdateUtils.GetPersistedUpdateTrack());
            UpdateTrackTimePicker.IsEnabled = true;
            UpdateTrackTimePicker.SelectedIndex = (int)TrackUpdateUtils.GetPersistedUpdateTrackTime();
        }

        async void RefreshPushEnabled(bool _appCenterEnabled)
        {
            PushEnabledSwitchCell.On = await Push.IsEnabledAsync();
            PushEnabledSwitchCell.IsEnabled = _appCenterEnabled;
        }

        async void UpdateEventFilterEnabled(object sender, ToggledEventArgs e)
        {
            if (EventFilterHolder.Implementation != null)
            {
                if (!_eventFilterStarted)
                {
                    AppCenter.Start(EventFilterHolder.Implementation.BindingType);
                    _eventFilterStarted = true;
                }
                await EventFilterHolder.Implementation.SetEnabledAsync(e.Value);
            }
        }

        public class CustomDocument
        {
            [JsonProperty("id")]
            public Guid? Id { get; set; }

            [JsonProperty("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonProperty("somenumber")]
            public int SomeNumber { get; set; }

            [JsonProperty("someprimitivearray")]
            public int[] SomePrimitiveArray { get; set; }

            [JsonProperty("someobjectarray")]
            public CustomDocument[] SomeObjectArray { get; set; }

            [JsonProperty("someprimitivecollection")]
            public IList SomePrimitiveCollection { get; set; }

            [JsonProperty("someobjectcollection")]
            public IList SomeObjectCollection { get; set; }

            [JsonProperty("someobject")]
            public Dictionary<string, Uri> SomeObject { get; set; }

            [JsonProperty("customdocument")]
            public CustomDocument Custom { get; set; }
        }
    }
}
