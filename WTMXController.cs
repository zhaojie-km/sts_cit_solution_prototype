using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WtMx6Lib;

namespace LineAuto
{
    class WTMXController 
    {
		//=====================================================================
		// 定数xx
		//=====================================================================
		public const int NORMAL_CHL_MAX = 60;   // 通常ﾁｬﾈﾙ最大数
		public const int WAIT_TIME = 3000; // 10ms単位

		/// <summary>
		/// デバイス制御ハンドル（Wtmx.INVALID_HANDLE = 未オープン）
		/// <para>■frmMain_Load実行時に設定、frmMain_FormClosed実行時にクリア</para>
		/// </summary>
		int m_hWtmx = Wtmx.INVALID_HANDLE;


		


		//=====================================================================
		// デバイス接続(アタッチ)監視
		//
		// Inp:
		// Out:
		// Ret: true  = アタッチ成功、あるいは既にアタッチ状態
		//      false = 未アタッチ、あるいはアタッチ失敗
		//=====================================================================
		private bool IsAttached()
		{
			// ﾃﾞﾊﾞｲｽ制御ﾊﾝﾄﾞﾙ作成(ｺｰﾙﾊﾞｯｸ機構無 & 非同期ｺﾏﾝﾄﾞ蓄積ﾁｬﾈﾙ個別方式)
			m_hWtmx = Wtmx.CreateDeviceHandle(0, 0, Wtmx.CMDBUFFER_DIVIDE);

		/// <summary>
		/// デバイスドライバ・ハンドル（Wtmx.INVALID_HANDLE_VALUE = 不正）
		/// <para>■デバイス接続時(IsAttached)に設定、デバイス取外(IsDetached)時にクリア</para>
		/// </summary>
		IntPtr hDevice = Wtmx.INVALID_HANDLE_VALUE;


		/// <summary>
		/// デバイス制御ハンドル情報
		/// <para>■デバイス接続時(IsAttached)に更新</para>
		/// </summary>
		Wtmx.DEVICE_HANDLE_INFO devInfo = new Wtmx.DEVICE_HANDLE_INFO();

		/// <summary>
		/// チャネル情報配列（システム・チャネル、および全通常チャネル数分を確保）
		/// <para>■システム・チャネル番号（インデックス0のnNumber）は、デバイス接続時(IsAttached)に更新</para>
		/// </summary>
		CHL_TBL m_chlInfo = new CHL_TBL(NORMAL_CHL_MAX);


		// ﾃﾞﾊﾞｲｽ制御ﾊﾝﾄﾞﾙ･ｱﾀｯﾁ
		int nStat = Wtmx.AttachDeviceHandle(m_hWtmx);

			bool bStat;
			switch (nStat)
			{
				case Wtmx.ATTACH_CONNECTED:
					//---------------------------------------------------------
					// アタッチ成功(デバイス接続検知)
					//---------------------------------------------------------

					// ﾃﾞﾊﾞｲｽﾄﾞﾗｲﾊﾞ･ﾊﾝﾄﾞﾙ保存
					Wtmx.GetOpenHandle(m_hWtmx, Wtmx.FUNC_DEVICE, 0, out hDevice);

					// ﾃﾞﾊﾞｲｽ制御ﾊﾝﾄﾞﾙ情報保存
					Wtmx.GetDeviceHandleInfo(m_hWtmx, devInfo);

					// ﾁｬﾈﾙ情報に、ｼｽﾃﾑ･ﾁｬﾈﾙ番号保存
					// ※特別なｼｽﾃﾑでない限り、ｼｽﾃﾑ･ﾁｬﾈﾙ番号は「61」固定
					m_chlInfo[0].nNumber = devInfo.dwSysChlNum;

					string strText = string.Format("==== デバイス接続 VocChl={0}, MovChl={1}",
												devInfo.dwMaxChlVoice,
												devInfo.dwMaxChlMovie);
					Console.WriteLine(strText);

					bStat = true;
					break;

				case Wtmx.ATTACH_ALREADY:
					//---------------------------------------------------------
					// 既にアタッチ状態
					//---------------------------------------------------------
					bStat = true;
					break;

				case Wtmx.ATTACH_NOTHING:
					//---------------------------------------------------------
					// 未アタッチ(デバイス未接続)
					//---------------------------------------------------------
					bStat = false;
					break;

				default:
					//---------------------------------------------------------
					// アタッチ失敗
					//---------------------------------------------------------
					int nErr = Wtmx.GetLastError();
					strText = string.Format("**** デバイス接続失敗 Stat={0}, Err={1}",
											nStat, nErr);
					Console.WriteLine(strText);

					bStat = false;
					break;
			}

			return bStat;
		}

		public void StartCall(String callNumber,int dwChlNum)
        {
			bool bStat;

			// 自動発信開始（通常発信）
			bStat = Wtmx.StartCall(

				m_hWtmx,       // デバイス制御ハンドル

				dwChlNum,　　　//チャンネル番号(TODO：空いてるチャンネル番号をチェックして代入する)

				callNumber,    // 発信先のダイヤル番号

				WAIT_TIME,     //ダイヤル発呼後から、相手応答されるまでの待機時間を１０ｍｓ単位で指定します。（０～６５５３５）

				0 ); //発信動作フラグ

			
			if (bStat)
			{

				// 開始成功
				Console.WriteLine("発信成功");

			}
            else
            {
				Console.WriteLine("発信失敗");
			}

		}


		public void ChanleConbi(string callNumbuer1, string callNumbuer2, int dwChlNum1, int dwChlNum2)
        {
			bool line1state;
			bool line2state;
			
			line1state = Wtmx.StartCall(
				m_hWtmx,       // デバイス制御ハンドル

				dwChlNum1,       //チャンネル番号(TODO：空いてるチャンネル番号をチェックして代入する)

				callNumbuer1,    // 発信先のダイヤル番号

				WAIT_TIME,       //ダイヤル発呼後から、相手応答されるまでの待機時間を１０ｍｓ単位で指定します。（０～６５５３５）

				0);              //発信動作フラグ
			
			line2state = Wtmx.StartCall(
			    m_hWtmx,       // デバイス制御ハンドル

				dwChlNum2,       //チャンネル番号(TODO：空いてるチャンネル番号をチェックして代入する)

				callNumbuer2,    // 発信先のダイヤル番号

				WAIT_TIME,       //ダイヤル発呼後から、相手応答されるまでの待機時間を１０ｍｓ単位で指定します。（０～６５５３５）

				0);              //発信動作フラグcallNumbuer1, dwChlNum1)

			
			int[] dwaChlList = new int[Wtmx.COMBINE_CHL_MAX];

			int[] dwaCmbList = new int[Wtmx.COMBINE_CHL_MAX];

			
			if (line1state)
			{
				dwaChlList[0] = dwChlNum1;
			}
			else {
				Console.WriteLine(callNumbuer1+"に発信失敗") ;
				return;
			}

			if (line2state)
			{
				dwaChlList[1] = dwChlNum2;
			}
			else
			{
				Console.WriteLine(callNumbuer2 + "に発信失敗");
				return;
			}

			
			Wtmx.CombineChannel(m_hWtmx, Wtmx.COMBINE_SET, 1,  //group no :1

						dwaChlList, 2,

						dwaCmbList);




		}


	}
}
