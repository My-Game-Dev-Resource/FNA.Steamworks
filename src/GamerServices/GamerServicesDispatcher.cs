﻿#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class GamerServicesDispatcher
	{
		#region Public Static Properties

		public static bool IsInitialized
		{
			get;
			private set;
		}

		public static IntPtr WindowHandle
		{
			get;
			set;
		}

		#endregion

		#region Private Static Variables

		private static object steamUpdateMutex = new object();

#pragma warning disable 0414
		private static Callback<GameOverlayActivated_t> overlayActivated;
		private static Callback<GamepadTextInputDismissed_t> textInputDismissed;
		private static Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
#pragma warning restore 0414

		#endregion

		#region Public Static Events

#pragma warning disable 0067
		// This should never happen, but lol XNA4 compliance -flibit
		public static event EventHandler<EventArgs> InstallingTitleUpdate;
#pragma warning restore 0067

		#endregion

		#region Public Static Methods

		public static void Initialize(IServiceProvider serviceProvider)
		{
			IsInitialized = SteamAPI.Init();

			if (!IsInitialized)
			{
				throw new GamerServicesNotAvailableException(
					"Steam is not running, please restart Steam!"
				);
			}

			AppDomain.CurrentDomain.ProcessExit += (o, e) =>
			{
				SteamAPI.Shutdown();
				IsInitialized = false;
			};

			overlayActivated = Callback<GameOverlayActivated_t>.Create(Guide.OnOverlayActivated);
			textInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(Guide.OnTextInputDismissed);
			lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(Net.NetworkSession.OnInviteAccepted);

			SteamUserStats.RequestCurrentStats();

			List<SignedInGamer> startGamers = new List<SignedInGamer>(1);
			startGamers.Add(new SignedInGamer(
				SteamUser.GetSteamID(),
				SteamFriends.GetPersonaName(),
				IsInitialized
			));

			// FIXME: This is stupid -flibit
			startGamers.Add(new SignedInGamer(
				SteamUser.GetSteamID(),
				SteamFriends.GetPersonaName() + " (1)",
				IsInitialized,
				true,
				PlayerIndex.Two
			));
			startGamers.Add(new SignedInGamer(
				SteamUser.GetSteamID(),
				SteamFriends.GetPersonaName() + " (2)",
				IsInitialized,
				true,
				PlayerIndex.Three
			));
			startGamers.Add(new SignedInGamer(
				SteamUser.GetSteamID(),
				SteamFriends.GetPersonaName() + " (3)",
				IsInitialized,
				true,
				PlayerIndex.Four
			));

			Gamer.SignedInGamers = new SignedInGamerCollection(startGamers);
			foreach (SignedInGamer gamer in Gamer.SignedInGamers)
			{
				SignedInGamer.OnSignIn(gamer);
			}
		}

		public static void Update()
		{
			lock (steamUpdateMutex)
			{
				SteamAPI.RunCallbacks();
			}

			// TODO: Guest hotplugging!
		}

		internal static bool UpdateAsync()
		{
			// If a thread's calling this after we quit, tell it to panic!
			if (IsInitialized)
			{
				Update();
			}
			return IsInitialized;
		}

		#endregion
	}
}
