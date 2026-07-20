using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalPike.PikeConsole.Config;
using FractalPike.PikeConsole.Core.RuntimeExecution;
using Godot;

namespace FractalPike.PikeConsole.Frontend.Controllers;

public partial class InputController : LineEdit
{
	[ExportGroup("Dependencies")]
	[Export] OutputController _outputController;
	[Export] ItemList _suggestionBox;

	[ExportGroup("Settings")]
	[Export] string _feedbackPrefix = "] ";


	// Suggestion box related
	CancellationTokenSource _debounceCts;

	bool _isBrowsingSelections = false;
	string _currentSearchTerm = string.Empty;

	// Command history cache
	readonly List<string> _commandHistory = [];
	int _historyIndex = 0;
	bool _isBrowsingHistory = false;
	bool _isApplyingHistory = false;


	public override void _EnterTree()
	{
		TextSubmitted += OnInputSubmitted;
		TextChanged += OnInputChanged;

		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;

		PikeConsoleStates.ConsoleHistorySize.ValueChanged += OnHistorySizeChanged;
	}

	public override void _ExitTree()
	{
		TextSubmitted -= OnInputSubmitted;
		TextChanged -= OnInputChanged;

		FocusEntered -= OnFocusEntered;
		FocusExited -= OnFocusExited;

		PikeConsoleStates.ConsoleHistorySize.ValueChanged -= OnHistorySizeChanged;
	}

	// AcceptEvent seems to be like preventdefault() in JS.
	// Idk if this is good or not. Feels brittle.
	public override void _GuiInput(InputEvent e)
	{
		if (e is InputEventKey keyEvent && keyEvent.Pressed)
		{
			bool suggestionsOpen = _suggestionBox != null && _suggestionBox.Visible;

			if (keyEvent.Keycode == Key.Up)
			{
				if (suggestionsOpen)
					NavigateSuggestions(-1);
				else if (string.IsNullOrEmpty(Text) || _isBrowsingHistory)
					NavigateHistory(-1);

				AcceptEvent();
			}
			else if (keyEvent.Keycode == Key.Down)
			{
				if (suggestionsOpen)
					NavigateSuggestions(1);
				else if (string.IsNullOrEmpty(Text) || _isBrowsingHistory)
					NavigateHistory(1);

				AcceptEvent();
			}
			else if (keyEvent.Keycode is Key.Left or Key.Right)
			{
				_isBrowsingHistory = false;
			}
			else if (keyEvent.Keycode == Key.Escape)
			{
				CloseSuggestions();
				AcceptEvent();
			}
			else if (keyEvent.Keycode is Key.Enter or Key.KpEnter)
			{
				if (_isBrowsingSelections)
					AutocompleteSelection();
				else
					OnInputSubmitted(Text);

				_suggestionBox.Hide();
				AcceptEvent();
			}
		}
	}

	void OnHistorySizeChanged(int newSize)
	{
		while (_commandHistory.Count > newSize && _commandHistory.Count > 0)
			_commandHistory.RemoveAt(0);

		if (_historyIndex > _commandHistory.Count)
			_historyIndex = _commandHistory.Count;
	}

	void OnFocusExited()
	{
		CloseSuggestions();
	}

	void OnFocusEntered()
	{
		HandleInputChanged(Text);
	}

	void OnInputSubmitted(string inputStatement)
	{
		_outputController.PushText($"{_feedbackPrefix}{inputStatement}\n");
		StatementExecutor.Execute(ExecutionSource.Standard, inputStatement);

		// Save to history
		if (!string.IsNullOrWhiteSpace(inputStatement))
			if (_commandHistory.Count == 0 || _commandHistory[^1] != inputStatement)
				_commandHistory.Add(inputStatement);

		if (_commandHistory.Count > PikeConsoleStates.ConsoleHistorySize.Value)
			_commandHistory.RemoveAt(0);

		// Reset all history states
		_historyIndex = _commandHistory.Count;
		_isBrowsingHistory = false;

		Clear();
	}

	async void OnInputChanged(string newText)
	{
		// If we type ANYTHING we are no longer in history mode.
		if (!_isApplyingHistory)
			_isBrowsingHistory = false;

		// Early check to see if we should just kill the suggestion box.
		// This is because we don't want the debounce make the input feel "laggy" when removing all text.
		if (string.IsNullOrWhiteSpace(newText) || _suggestionBox == null)
		{
			_suggestionBox?.Hide();
			return;
		}

		// Reused / repurposed debounce logic from the CVar save system.
		_debounceCts?.Cancel();

		_debounceCts = new();
		var tempToken = _debounceCts.Token;

		try
		{
			await Task.Delay(PikeConsoleStates.ConsoleSuggestionsDebounceMs.Value, tempToken);
			HandleInputChanged(newText);
		}
		catch (Exception) { }
	}

	/* 
	 * I gotta chill, but note to self for future cool stuff: 
	 * We could change IRuntimeExecutable (or just Command) to include "passthrough suggestion"
	 * Then check in here if [0] == signature && isPassthrough. Then stuff like help would be able to autocomplete commands as arguments.
	 * It would require a whole extra parser, but it would be cool. This is sufficient though!
	 * 
	 * More realistic: We could add icons later for: Command, CVar, Alias
	*/
	void HandleInputChanged(string newText)
	{
		var statements = StatementParser.ParseLine(newText);

		// Edge case if someone starts the input with semicolon or some stupid shenanigans.
		if (statements.Length < 1)
		{
			CloseSuggestions();
			return;
		}

		string signature = statements[^1].Signature;
		_currentSearchTerm = signature;

		string[] matches = RegistryBrowser.FindSignatures(signature, includeAliases: true);
		_suggestionBox.Clear();

		if (matches.Length < 1 || matches[0].Equals(signature, StringComparison.OrdinalIgnoreCase))
		{
			CloseSuggestions();
			return;
		}

		int count = Mathf.Min(matches.Length, PikeConsoleStates.ConsoleMaxSuggestions.Value);
		for (int i = 0; i < count; i++)
		{
			_suggestionBox.AddItem(matches[i].ToLower());
		}

		_suggestionBox.Show();
	}


	// ----- ----- ----- ----- ----- 
	// 		HISTORY NAVIGATION
	// ----- ----- ----- ----- ----- 
	void NavigateHistory(int step)
	{
		if (_commandHistory.Count == 0)
			return;

		_isBrowsingHistory = true;
		_historyIndex += step;

		if (_historyIndex < 0)
			_historyIndex = 0;
		else if (_historyIndex >= _commandHistory.Count)
		{
			_historyIndex = _commandHistory.Count;
			_isBrowsingHistory = false;

			_isApplyingHistory = true;
			Text = string.Empty;
			_isApplyingHistory = false;
			return;
		}

		_isApplyingHistory = true;
		Text = _commandHistory[_historyIndex];
		CaretColumn = Text.Length;
		_isApplyingHistory = false;
	}

	// ----- ----- ----- ----- ----- 
	// 	SUGGESTION BOX NAVUGATION
	// ----- ----- ----- ----- ----- 

	void NavigateSuggestions(int step)
	{
		if (_suggestionBox.ItemCount == 0)
			return;

		// Set the flag so that other methods know we are currently within the suggestions box with the selection.
		_isBrowsingSelections = true;

		// ItemList can only return an array so we fetch that.
		// ItemLists are basically like our CVarEnums - a text list with a number.
		// So selected[0] is basically "the index value of the "first" selected item"
		// We always select just 1 item though. So it's "the" item for us.
		int[] selected = _suggestionBox.GetSelectedItems();
		int currentIndex = selected.Length > 0 ? selected[0] : -1;

		int nextIndex = currentIndex + step;

		if (nextIndex < 0)
			nextIndex = _suggestionBox.ItemCount - 1;
		else if (nextIndex >= _suggestionBox.ItemCount)
			nextIndex = 0;

		// Deselect all (because it's an array)
		// Then move the selection to the one item we want
		_suggestionBox.DeselectAll();
		_suggestionBox.Select(nextIndex);

		// Not necessary, but this auto scrolls to show if we are overflowing.
		_suggestionBox.EnsureCurrentIsVisible();
	}

	void AutocompleteSelection()
	{
		if (_suggestionBox.ItemCount == 0)
			return;

		// Set the flag so that other methods know we are no longer within the suggestions box with the selection.
		_isBrowsingSelections = false;

		int[] selected = _suggestionBox.GetSelectedItems();

		// Whatever we have selected (because we only select 1 at a time) is what we want to apply.
		// selected[0] is just "THE" selection. We use [0] because the itemlist always returns an array.
		int indexToUse = selected.Length > 0 ? selected[0] : 0;
		string autoCompleteSignature = _suggestionBox.GetItemText(indexToUse) + " ";

		// We get the LAST index in the input text for the half completed search term (that's where we're currently writing)
		// Then we cut our half-finnished signature and add the complete signature.
		int replaceIndex = Text.LastIndexOf(_currentSearchTerm, StringComparison.OrdinalIgnoreCase);
		if (replaceIndex >= 0)
			Text = Text[..replaceIndex] + autoCompleteSignature;
		else
			Text = autoCompleteSignature;

		// Place the carret at the end and close the suggestions.
		CaretColumn = Text.Length;
		CloseSuggestions();
	}

	void CloseSuggestions()
	{
		_isBrowsingSelections = false;
		_suggestionBox.Clear();
		_suggestionBox.Hide();
	}
}
