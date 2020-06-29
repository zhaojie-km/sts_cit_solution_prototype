using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using WtMx6Lib;

namespace LineAuto
{
	///////////////////////////////////////////////////////////////////////////
	// 列挙型
	///////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// チャネル回線状態列挙型
	/// </summary>
	public enum CHLSTAT : int
	{
		/// <summary>
		/// チャネル未接続状態（デバイス未接続）
		/// </summary>
		Remove = 0,
		
		/// <summary>
		/// チャネル接続状態
		/// </summary>
		Ready,
		
		/// <summary>
		/// チャネル起動状態
		/// </summary>
		Start,
	};

	/// <summary>
	/// チャネル稼動状態列挙型
	/// </summary>
	public enum CHLSTAGE : int
	{
		/// <summary>
		/// 次処理進行
		/// </summary>
		NextProc = -1,

		/// <summary>
		/// 待機状態（着信/発信待機）
		/// </summary>
		Waiting = 0,
		
		/// <summary>
		/// 自動発信
		/// </summary>
		Calling = 10,
		
		/// <summary>
		/// 回線応答有
		/// </summary>
		Connect = 20,
		
		/// <summary>
		/// 冒頭ガイダンス再生
		/// </summary>
		HeadPlay = 30,

		/// <summary>
		/// ダイヤル入力
		/// </summary>
		DialInp = 40,

		/// <summary>
		/// ダイヤル入力後音声再生
		/// </summary>
		DialPlay = 50,

		/// <summary>
		/// 回線開放（回線切断）
		/// </summary>
		Disconnect = 100,
	};


	///////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// チャネル情報配列クラス
	/// <para>■全通常チャネルの情報（システム・チャネルを除く）を、チャネル別に配列形式で管理しています。</para>
	/// <para>■単にチャネル個別の管理情報を定義しているだけであり、本クラス単独でチャネル制御が行われる訳ではありません。</para>
	/// <para>（常にfrmMainクラスから相互的に使用されます）</para>
	/// <para>■なお、スレッドセーフは保証されません。</para>
	/// </summary>
	///////////////////////////////////////////////////////////////////////////
	public class CHL_TBL
	{
		//=====================================================================
		// フィールド
		//=====================================================================

		// ﾁｬﾈﾙ情報配列(ｲﾝﾃﾞｯｸｽ0 = ｼｽﾃﾑ･ﾁｬﾈﾙ, ｲﾝﾃﾞｯｸｽ1～ = ﾁｬﾈﾙ番号1～)
		// ※通常は本ｸﾗｽのｲﾝﾃﾞｸｻを使用することで、ﾁｬﾈﾙ番号をそのまま配列要素に
		// 指定することができます。
		private CHL_TBL.INFO[] m_chlInfoTbl;

		//=====================================================================
		// コンストラクタ
		//=====================================================================

		/// <summary>
		/// 最大チャネル回線数を指定して、チャネル情報配列を作成するコンストラクタ
		/// <para>■指定された回数数＋１個（システム・チャネル用）の配列が確保されます。</para>
		/// </summary>
		/// <param name="nMaxLine">チャネル回線数 (システム・チャネルを除いた、通常チャネルの最大数)</param>
		public CHL_TBL(int nMaxLine)
		{
			if (nMaxLine <= 0)
			{
				throw(new ApplicationException("指定したチャネル回線数が不正です。"));
			}

			// 指定されたﾁｬﾈﾙ回線数+「ｼｽﾃﾑ･ﾁｬﾈﾙ」分のﾁｬﾈﾙ情報配列を作成
			m_chlInfoTbl = new CHL_TBL.INFO[nMaxLine + 1];
            for (int i = 0; i < m_chlInfoTbl.Length; i++)
			{
				// 先頭ｲﾝﾃﾞｯｸｽ(ｼｽﾃﾑ･ﾁｬﾈﾙ用)のﾁｬﾈﾙ番号は初期値として「0」を設定
				m_chlInfoTbl[i] = new CHL_TBL.INFO(i);
			}
		}

		//=====================================================================
		// プロパティ
		//=====================================================================

		/// <summary>
		/// チャネル情報配列要素数
		/// </summary>
		public int Length
		{
			get
			{
				return m_chlInfoTbl.Length;
			}
		}

		/// <summary>
		/// チャネル情報配列フィールド参照
		/// </summary>
		public CHL_TBL.INFO[] Items
		{
			get
			{
				return m_chlInfoTbl;
			}
		}
		
		/// <summary>
		/// 最終チャネル回線番号（最大通常チャネル番号）
		/// </summary>
		public int LastNumber
		{
			get
			{
				return m_chlInfoTbl[m_chlInfoTbl.Length - 1].nNumber;
			}
		}
		
		//=====================================================================
		// インデクサ
		//=====================================================================

		/// <summary>
		/// チャネル番号に該当するチャネル情報参照
		/// <para>■不正チャネル番号指定時は、nullが返ります。</para>
		/// </summary>
		public CHL_TBL.INFO this[int nChlNum]
		{
			get
			{
				// ｼｽﾃﾑ･ﾁｬﾈﾙ番号指定時は、先頭ｲﾝﾃﾞｯｸｽを参照
				if (nChlNum == m_chlInfoTbl[0].nNumber) nChlNum = 0;
				
				return (nChlNum >= 0 && nChlNum < m_chlInfoTbl.Length ?
						m_chlInfoTbl[nChlNum] : null);
			}
		}

		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// チャネル情報アイテム・クラス
		/// <para>■チャネル個別の管理情報を定義しています。</para>
		/// </summary>
		///////////////////////////////////////////////////////////////////////
		public class INFO
		{
			//=================================================================
			// フィールド
			// ※システム・チャネル用は、チャネル番号(nNumber)とチャネル情報表
			// 示用リストビュー(lstMon)のみ使用さます。
			//=================================================================

			/// <summary>
			/// チャネル番号 (1～)
			/// </summary>
			public int nNumber;

			/// <summary>
			/// チャネル回線状態
			/// </summary>
			public CHLSTAT nStat;

			/// <summary>
			/// チャネル稼動状態
			/// <para>■チャネル回線状態(nStat)が、CHLSTAT.Start以上のとき有効</para>
			/// </summary>
			public CHLSTAGE nStage;

			/// <summary>
			/// 音声再生ファイル番号
			/// </summary>
			public int nPlayNum;

			/// <summary>
			/// ダイヤル入力番号
			/// </summary>
			public string strDialNum;

			/// <summary>
			/// チャネル情報表示用リストビュー
			/// </summary>
			public ListView lstMon;

			//-----------------------------------------------------------------
			// 以下は、自動応答用パラメータ
			// ※回線応答時に、チャネル制御タブ画面の内容が設定されます。
			//-----------------------------------------------------------------

			/// <summary>
			/// 音声再生ファイル名配列
			/// <para>■インデックス0～ = 音声ファイル#0～</para>
			/// </summary>
			public string[] p_strPlayFileTbl;
			
			/// <summary>
			/// ダイヤル入力桁数
			/// </summary>
			public int p_nDialLen;

			/// <summary>
			/// ダイヤル入力待機時間[10ms]
			/// </summary>
			public int p_nDialTime;
			
			/// <summary>
			/// ダイヤル入力信号音フラグ（true = 有効）
			/// </summary>
			public bool p_bDialSound;
			
			/// <summary>
			/// 再生ダイヤル番号配列
			/// <para>■インデックス0～ = 再生ダイヤル番号#1～</para>
			/// </summary>
			public string[] p_strPlayDialTbl;
			

			//=================================================================
			/// <summary>
			/// チャネル番号を設定するコンストラクタ
			/// </summary>
			/// <param name="nChlNum">チャネル番号 (1～)</param>
			//=================================================================
			public INFO(int nChlNum)
			{
				nNumber = nChlNum;
				nStat = CHLSTAT.Remove;
				nStage = CHLSTAGE.Waiting;
				
				nPlayNum = 0;
				strDialNum = "";
				lstMon = null;
				
				p_strPlayFileTbl = new string[5];
				p_nDialLen = 0;
				p_nDialTime = 0;
				p_bDialSound = false;
				p_strPlayDialTbl = new string[3];
			}

			//=================================================================
			// プロパティ
			//=================================================================

		}// class INFO
		
	}// class CHL_TBL
}
