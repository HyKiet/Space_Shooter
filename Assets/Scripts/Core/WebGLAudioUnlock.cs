using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// WebGL Audio Unlock — Fix âm thanh không phát trên browser
///
/// Browser modern chặn autoplay audio cho đến khi user tương tác.
/// Script này inject JS để resume AudioContext ngay khi user click/touch.
///
/// Chỉ chạy trên WebGL, không ảnh hưởng standalone/editor.
/// Attach vào bất kỳ GameObject nào (khuyến nghị: GameManager).
/// </summary>
public class WebGLAudioUnlock : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UnlockAudioContext();

    private static bool _unlocked = false;

    private void Awake()
    {
        // Inject JS function để resume AudioContext khi user click
        Application.ExternalEval(@"
            window._unityAudioUnlocked = false;
            function _tryUnlockAudio() {
                if (window._unityAudioUnlocked) return;
                if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
                    if (WEBAudio.audioContext.state === 'suspended') {
                        WEBAudio.audioContext.resume().then(function() {
                            window._unityAudioUnlocked = true;
                            console.log('[Unity] AudioContext resumed!');
                        });
                    } else {
                        window._unityAudioUnlocked = true;
                    }
                }
            }
            document.addEventListener('click',     _tryUnlockAudio, { once: false });
            document.addEventListener('touchstart', _tryUnlockAudio, { once: false });
            document.addEventListener('keydown',   _tryUnlockAudio, { once: false });
            console.log('[Unity] WebGL Audio Unlock listeners registered.');
        ");
    }
#endif

    // ── Fallback cho Editor / Standalone (không làm gì) ──
    private void Start()
    {
        // Không cần gì thêm
    }
}
