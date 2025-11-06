using UnityEngine;



using TMPro;



//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

/// <summary>

/// メインクラス.

/// </summary>

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX



public sealed class Main : MonoBehaviour

{

//================================================================================

// Fields.

//================================================================================



/// <summary>

/// ラベル.

/// </summary>

[SerializeField] private TextMeshProUGUI Label = default;


//================================================================================

// Methods.

//================================================================================



//--------------------------------------------------------------------------------

// Event methods.

//--------------------------------------------------------------------------------



/// <summary>

/// Intの値が返るイベント.

/// </summary>

/// <param name="value">値.</param>

public void OnIntEvent(int value) => Label.SetText($"{value}");

}