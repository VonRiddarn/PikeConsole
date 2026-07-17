using System;
using System.Threading;
using System.Threading.Tasks;
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
	[Export] int _maxSuggestions = 5;
	[Export] int _suggestionsDebounceMs = 200;


	CancellationTokenSource _debounceCts;

	bool _isBrowsingSelections = false;
	string _currentSearchTerm = string.Empty;

	public override void _EnterTree()
	{
		TextSubmitted += OnInputSubmitted;
		TextChanged += OnInputChanged;

		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;
	}

	public override void _ExitTree()
	{
		TextSubmitted -= OnInputSubmitted;
		TextChanged -= OnInputChanged;

		FocusEntered -= OnFocusEntered;
		FocusExited -= OnFocusExited;
	}

	// AcceptEvent seems to be like preventdefault() in JS.
	// Idk if this is good or not. Feels brittle.
	public override void _GuiInput(InputEvent e)
	{
		// TODO: Add command history here. Up = previous command.
		if (_suggestionBox == null || !_suggestionBox.Visible)
			return;

		if (e is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Up)
			{
				NavigateSuggestions(-1);
				AcceptEvent();
			}
			else if (keyEvent.Keycode == Key.Down)
			{
				NavigateSuggestions(1);
				AcceptEvent();
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

	private void OnFocusExited()
	{
		CloseSuggestions();
	}

	private void OnFocusEntered()
	{
		HandleInputChanged(Text);
	}

	void OnInputSubmitted(string inputStatement)
	{
		_outputController.PushText($"{_feedbackPrefix}{inputStatement}\n");
		StatementExecutor.Execute(ExecutionSource.Standard, inputStatement);
		Clear();
	}

	async void OnInputChanged(string newText)
	{
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
			await Task.Delay(_suggestionsDebounceMs, tempToken);
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

		int count = Mathf.Min(matches.Length, _maxSuggestions);
		for (int i = 0; i < count; i++)
		{
			_suggestionBox.AddItem(matches[i].ToLower());
		}

		_suggestionBox.Show();
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
