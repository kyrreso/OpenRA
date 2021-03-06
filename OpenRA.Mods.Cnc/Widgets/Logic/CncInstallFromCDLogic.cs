#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenRA.FileFormats;
using OpenRA.Widgets;

namespace OpenRA.Mods.Cnc.Widgets.Logic
{
	public class CncInstallFromCDLogic
	{
		Widget panel;
		ProgressBarWidget progressBar;
		LabelWidget statusLabel;
		Action afterInstall;
		ButtonWidget retryButton, backButton;
		Widget installingContainer, insertDiskContainer;

		string[] filesToCopy, filesToExtract;

		[ObjectCreator.UseCtor]
		public CncInstallFromCDLogic([ObjectCreator.Param] Widget widget,
			[ObjectCreator.Param] Action afterInstall,
			[ObjectCreator.Param] string[] filesToCopy,
			[ObjectCreator.Param] string[] filesToExtract)
		{
			this.afterInstall = afterInstall;
			this.filesToCopy = filesToCopy;
			this.filesToExtract = filesToExtract;

			panel = widget.GetWidget("INSTALL_FROMCD_PANEL");
			progressBar = panel.GetWidget<ProgressBarWidget>("PROGRESS_BAR");
			statusLabel = panel.GetWidget<LabelWidget>("STATUS_LABEL");

			backButton = panel.GetWidget<ButtonWidget>("BACK_BUTTON");
			backButton.OnClick = Widget.CloseWindow;

			retryButton = panel.GetWidget<ButtonWidget>("RETRY_BUTTON");
			retryButton.OnClick = CheckForDisk;

			installingContainer = panel.GetWidget("INSTALLING");
			insertDiskContainer = panel.GetWidget("INSERT_DISK");
			CheckForDisk();
		}

		public static bool IsValidDisk(string diskRoot)
		{
			var files = new string[][] {
				new [] { diskRoot, "CONQUER.MIX" },
				new [] { diskRoot, "DESERT.MIX" },
				new [] { diskRoot, "INSTALL", "SETUP.Z" },
			};

			return files.All(f => File.Exists(f.Aggregate(Path.Combine)));
		}

		void CheckForDisk()
		{
			var path = InstallUtils.GetMountedDisk(IsValidDisk);

			if (path != null)
				Install(path);
			else
			{
				insertDiskContainer.IsVisible = () => true;
				installingContainer.IsVisible = () => false;
			}
		}

		void Install(string source)
		{
			backButton.IsDisabled = () => true;
			retryButton.IsDisabled = () => true;
			insertDiskContainer.IsVisible = () => false;
			installingContainer.IsVisible = () => true;

			var dest = new string[] { Platform.SupportDir, "Content", "cnc" }.Aggregate(Path.Combine);
			var extractPackage = "INSTALL/SETUP.Z";

			var installCounter = 0;
			var installTotal = filesToExtract.Count() + filesToExtract.Count();
			var onProgress = (Action<string>)(s => Game.RunAfterTick(() =>
			{
				progressBar.Percentage = installCounter*100/installTotal;
				installCounter++;

				statusLabel.GetText = () => s;
			}));

			var onError = (Action<string>)(s => Game.RunAfterTick(() =>
			{
				statusLabel.GetText = () => "Error: "+s;
				backButton.IsDisabled = () => false;
				retryButton.IsDisabled = () => false;
			}));

			var t = new Thread( _ =>
			{
				try
				{
					if (!InstallUtils.CopyFiles(source, filesToCopy, dest, onProgress, onError))
						return;

					if (!InstallUtils.ExtractFromPackage(source, extractPackage, filesToExtract, dest, onProgress, onError))
				    	return;

					Game.RunAfterTick(() =>
					{
						Widget.CloseWindow();
						afterInstall();
					});
				}
				catch
				{
					onError("Installation failed");
				}
			}) { IsBackground = true };
			t.Start();
		}
	}
}
