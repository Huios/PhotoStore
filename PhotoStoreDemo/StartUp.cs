﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Data;
using System.IO;
using System.Windows;
using System.Runtime.CompilerServices;

namespace PhotoStoreDemo
{
    public class StartUp
    {
        [STAThread]
        public static void Main(string[] cmdArgs)
        {
            //Non-packaged version of app running
            if (!ExecutionMode.IsRunningWithIdentity())
            {
                    Debug.WriteLine("Running non-packaged version");
                    SingleInstanceManager wrapper = new SingleInstanceManager();
                    wrapper.Run(cmdArgs);

            }
            else //App is packaged and running with identity, handle launch and other activation typed
            {
                //Handle Packaged Activation e.g Share target activation or clicking on a Tile
                // Launching the .exe directly will have activationArgs == null
                var activationArgs = AppInstance.GetActivatedEventArgs();
                if (activationArgs != null)
                {
                    switch (activationArgs.Kind)
                    {
                        case ActivationKind.Launch:
                            HandleLaunch(activationArgs as LaunchActivatedEventArgs);
                            break;
                        case ActivationKind.ToastNotification:
                            HandleToastNotification(activationArgs as ToastNotificationActivatedEventArgs);
                            break;
                        case ActivationKind.ShareTarget:
                            HandleShareAsync(activationArgs as ShareTargetActivatedEventArgs);
                            break;
                        default:
                            HandleLaunch(null);
                            break;
                    }

                }
                //This is a direct exe based launch e.g. double click app .exe or desktop shortcut pointing to .exe
                else
                {
                    SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
                    singleInstanceManager.Run(cmdArgs);
                }
            }

        }

        static void HandleLaunch(LaunchActivatedEventArgs args)
        {

            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
            Debug.Indent();
            Debug.WriteLine("WPF App using a Sparse Package");


            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());
        }

        static void HandleToastNotification(ToastNotificationActivatedEventArgs args)
        {
            ValueSet userInput = args.UserInput;
            string pathFromToast = userInput["textBox"].ToString();

            ImageFile item = new ImageFile(pathFromToast);
            item.AddToCache();

            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());
        }

        static async void HandleShareAsync(ShareTargetActivatedEventArgs args)
        {

            ShareOperation shareOperation = args.ShareOperation;
            if (shareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                try
                {
                    Stream bitMapStream = (Stream)shareOperation.Data.GetBitmapAsync();
                    ImageFile image = new ImageFile(bitMapStream);
                    image.AddToCache();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (shareOperation.Data.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                try
                {
                    IReadOnlyList<IStorageItem> items = await shareOperation.Data.GetStorageItemsAsync();
                    IStorageFile file = (IStorageFile)items[0];
                    string path = file.Path;

                    ImageFile image = new ImageFile(path);
                    image.AddToCache();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            shareOperation.ReportCompleted();
            SingleInstanceManager singleInstanceManager = new SingleInstanceManager();
            singleInstanceManager.Run(Environment.GetCommandLineArgs());

            
           
        }

    }
}
