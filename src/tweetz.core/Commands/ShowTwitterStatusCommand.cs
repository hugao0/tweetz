﻿using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using tweetz.core.Infrastructure;
using twitter.core.Models;

namespace tweetz.core.Commands
{
    internal class ShowTwitterStatusCommand : ICommandBinding
    {
        public static readonly RoutedCommand Command = new RoutedUICommand();
        private IMessageBoxService MessageBoxService { get; }

        public ShowTwitterStatusCommand(IMessageBoxService messageBoxService)
        {
            MessageBoxService = messageBoxService;
        }

        public CommandBinding CommandBinding()
        {
            return new CommandBinding(Command, CommandHandler);
        }

        private void CommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            CommandHandlerAsync(e).ConfigureAwait(false);
        }

        private async ValueTask CommandHandlerAsync(ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is TwitterStatus status)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(status, options);
                await MessageBoxService.ShowMessageBoxAsync(json).ConfigureAwait(false);
            }
        }
    }
}