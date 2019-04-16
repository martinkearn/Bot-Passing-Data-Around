// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using DataPassingBot.Dialogs.Country;
using DataPassingBot.Dialogs.NameAge;
using DataPassingBot.Dialogs.Root.Resources;
using DataPassingBot.Resources;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPassingBot.Dialogs.Root
{
    public class RootDialog : ComponentDialog
    {
        private const string ChoicePromptName = "choiceprompt";

        public RootDialog(UserState userState) : base(nameof(RootDialog))
        {
            // Define the steps of the waterfall dialog and add it to the set.
            var waterfallSteps = new WaterfallStep[]
            {
                SayHiAsync,
                PromptForFlowAsync,
                HandleFlowResultAsync,
                EndAsync,
            };

            // Child dialogs
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(ChoicePromptName));
            AddDialog(new NameAgeDialog(userState));
            AddDialog(new CountryDialog(userState));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SayHiAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If required, get the utterance like this: 
            var utterance = (string)stepContext.Options;

            await stepContext.Context.SendActivityAsync($"{RootStrings.Welcome}", cancellationToken: cancellationToken);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PromptForFlowAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(ChoicePromptName,
                new PromptOptions
                {
                    Choices = ChoiceFactory.ToChoices(new List<string> { RootStrings.NameAgePrompt, RootStrings.CountryPrompt }),
                    Prompt = MessageFactory.Text(RootStrings.WhichFlowPrompt),
                    RetryPrompt = MessageFactory.Text(SharedStrings.InvalidResponseToChoicePrompt)
                },
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> HandleFlowResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = ((FoundChoice)stepContext.Result).Value;

            // A switch statement would be better but that requires a constant and we want all strings to be in RESX files
            if (result == RootStrings.NameAgePrompt)
            {
                return await stepContext.BeginDialogAsync(nameof(NameAgeDialog), cancellationToken);
            }
            else if (result == RootStrings.CountryPrompt)
            {
                return await stepContext.BeginDialogAsync(nameof(CountryDialog), cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the root dialog
            return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
