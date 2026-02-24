using Godot;
using System;
using System.Threading.Tasks;

public partial class EventCheckMessage : Control
{
	private RichTextLabel _messageLabel;
    private Button _confirmButton;
    
    private Tween _currentTween;
    private TaskCompletionSource<bool> _taskCompletionSource;
    private string _fullMessage = "";
    private int _currentCharIndex = 0;

	
	public override void _Ready()
	{
		// 获取节点引用（根据你的实际节点结构调整）
        _messageLabel = GetNode<RichTextLabel>("MarginContainer/Panel/VBoxContainer/Message");
                
        // 初始状态
        Visible = false;
        _messageLabel.Text = "";
	}

	/// <summary>
    /// 显示弹窗，并逐字显示消息
    /// </summary>
    /// <param name="message">要显示的消息（支持BBCode）</param>
	/// <param name="charInterval">逐字显示间隔（秒）</param>
    public async Task Show(string message, float charInterval = 0.05f)
    {
        _fullMessage = message + "\n";
        _currentCharIndex = 0;
        _messageLabel.Text = "";
        
        // 显示弹窗
        await ShowWindow();
        
        // 逐字显示动画
        while (_currentCharIndex < _fullMessage.Length)
        {
            // 处理BBCode标签，一次显示一个完整的标签
            if (_fullMessage[_currentCharIndex] == '[')
            {
                int endTagIndex = _fullMessage.IndexOf(']', _currentCharIndex);
                if (endTagIndex != -1)
                {
                    // 一次性显示整个标签
                    _messageLabel.Text += _fullMessage.Substring(_currentCharIndex, endTagIndex - _currentCharIndex + 1);
                    _currentCharIndex = endTagIndex + 1;
                    continue;
                }
            }
            
            // 显示单个字符
            _messageLabel.Text += _fullMessage[_currentCharIndex];
            _currentCharIndex++;
            
            // 等待一小段时间
            await ToSignal(GetTree().CreateTimer(charInterval), Timer.SignalName.Timeout);
        }
		await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);
    }

	/// <summary>
    /// 在现有消息后面追加新消息（逐字显示）
    /// </summary>
    public async Task Append(string message, float charInterval = 0.05f)
    {
        _fullMessage += message + "\n";
        
        while (_currentCharIndex < _fullMessage.Length)
        {
            // 处理BBCode标签
            if (_fullMessage[_currentCharIndex] == '[')
            {
                int endTagIndex = _fullMessage.IndexOf(']', _currentCharIndex);
                if (endTagIndex != -1)
                {
                    _messageLabel.Text += _fullMessage.Substring(_currentCharIndex, endTagIndex - _currentCharIndex + 1);
                    _currentCharIndex = endTagIndex + 1;
                    continue;
                }
            }
            
            _messageLabel.Text += _fullMessage[_currentCharIndex];
            _currentCharIndex++;
            
            await ToSignal(GetTree().CreateTimer(charInterval), Timer.SignalName.Timeout);
        }

		await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);
    }

	/// <summary>
    /// 清空消息
    /// </summary>
    public void Clear()
    {
        _fullMessage = "";
        _currentCharIndex = 0;
        _messageLabel.Text = "";
    }

	/// <summary>
    /// 显示弹窗（淡入动画）
    /// </summary>
    private async Task ShowWindow()
    {
        if (Visible) return;
        
        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
        Scale = new Vector2(0.8f, 0.8f);
        
        _currentTween = CreateTween();
        _currentTween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);
        _currentTween.Parallel().TweenProperty(this, "scale", Vector2.One, 0.3f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        
        await ToSignal(_currentTween, Tween.SignalName.Finished);
    }

	/// <summary>
    /// 隐藏弹窗（淡出动画）
    /// </summary>
    public async Task HideWindow()
    {
        if (!Visible) return;
        
        _currentTween = CreateTween();
        _currentTween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
        _currentTween.Parallel().TweenProperty(this, "scale", new Vector2(0.8f, 0.8f), 0.2f);
        
        await ToSignal(_currentTween, Tween.SignalName.Finished);
        
        Visible = false;
        _confirmButton.Hide();
        Clear();
    }

	/// <summary>
    /// 强制关闭弹窗
    /// </summary>
    public void ForceClose()
    {
        _taskCompletionSource?.SetResult(false);
        Visible = false;
        Clear();
    }
}
