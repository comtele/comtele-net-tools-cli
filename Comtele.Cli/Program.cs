using Comtele.Sdk.Services;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Text;

namespace Comtele.Cli
{
    public class Program
    {
        const string HELP_FLAGS = "-?|-h|--help";

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            BuildSendCommand(app);
            BuildScheduleCommand(app);
            BuildContextSendCommand(app);
            BuildContextScheduleCommand(app);
            BuildReplyReportCommand(app);
            BuildDetailedReportCommand(app);

            return app.Execute(args);
        }

        private static void BuildSendCommand(CommandLineApplication app)
        {
            app.Command("send", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var sender = command.Option("-s|--sender", "Remetente da mensagem que será enviada.", CommandOptionType.SingleValue);
                var content = command.Option("-c|--content", "Conteúdo da mensagem que será enviada.", CommandOptionType.SingleValue);
                var receiver = command.Option("-r|--receiver", "Destinatário da mensagem que será enviada.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var textMessageService = new TextMessageService(key.Value());
                    var result = textMessageService.Send(sender.Value(), content.Value(), new string[] { receiver.Value() });
                    Console.WriteLine(result.Message);
                });
            });
        }

        private static void BuildScheduleCommand(CommandLineApplication app)
        {
            app.Command("schedule", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var date = command.Option("-d|--date", "Data para agendar a mensagem que será enviada.", CommandOptionType.SingleValue);
                var sender = command.Option("-s|--sender", "Remetente da mensagem que será enviada.", CommandOptionType.SingleValue);
                var content = command.Option("-c|--content", "Conteúdo da mensagem que será enviada.", CommandOptionType.SingleValue);
                var receiver = command.Option("-r|--receiver", "Destinatário da mensagem que será enviada.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var textMessageService = new TextMessageService(key.Value());
                    var result = textMessageService.Schedule(sender.Value(), content.Value(), DateTime.Parse(date.Value()), new string[] { receiver.Value() });
                    Console.WriteLine(result.Message);
                });
            });
        }

        private static void BuildContextSendCommand(CommandLineApplication app)
        {
            app.Command("contextsend", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var sender = command.Option("-s|--sender", "Remetente da mensagem que será enviada.", CommandOptionType.SingleValue);
                var context = command.Option("-c|--context", "Nome da regra da mensagem que será enviada.", CommandOptionType.SingleValue);
                var receiver = command.Option("-r|--receiver", "Destinatário da mensagem que será enviada.", CommandOptionType.SingleValue);
                var date = command.Option("-d|--date", "Data para agendar a mensagem que será enviada.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var contextMessageService = new ContextMessageService(key.Value());
                    var result = contextMessageService.Schedule(sender.Value(), context.Value(), DateTime.Parse(date.Value()), new string[] { receiver.Value() });
                    Console.WriteLine(result.Message);
                });
            });
        }

        private static void BuildContextScheduleCommand(CommandLineApplication app)
        {
            app.Command("contextschedule", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var sender = command.Option("-s|--sender", "Remetente da mensagem que será enviada.", CommandOptionType.SingleValue);
                var context = command.Option("-c|--context", "Nome da regra da mensagem que será enviada.", CommandOptionType.SingleValue);
                var receiver = command.Option("-r|--receiver", "Destinatário da mensagem que será enviada.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var contextMessageService = new ContextMessageService(key.Value());
                    var result = contextMessageService.Send(sender.Value(), context.Value(), new string[] { receiver.Value() });
                    Console.WriteLine(result.Message);
                });
            });
        }

        private static void BuildReplyReportCommand(CommandLineApplication app)
        {
            app.Command("replies", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var startDate = command.Option("-s|--startdate", "Data inicial do relatório de respostas.", CommandOptionType.SingleValue);
                var endDate = command.Option("-e|--enddate", "Data final do relatório de respostas.", CommandOptionType.SingleValue);
                var file = command.Option("-f|--file", "Nome do arquivo que as respostas serão salvas", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    Console.WriteLine("Buscando respostas...");
                    var replyService = new ReplyService(key.Value());
                    var result = replyService.GetReport(DateTime.Parse(startDate.Value()), DateTime.Parse(endDate.Value()));

                    if (result.Success)
                    {
                        Console.WriteLine("Processando respostas...");
                        var builder = new StringBuilder();
                        builder.AppendLine($"SENDER;SENDER NAME;RECEIVED DATE;RECEIVED CONTENT;SENT CONTENT");
                        foreach (var reply in result.Object)
                        {
                            builder.AppendLine($"{reply.Sender};{reply.SenderName};{reply.ReceivedDate};{reply.ReceivedContent};{reply.SentContent}");
                        }

                        string fileName = "replies.csv";
                        if (file.HasValue())
                        {
                            fileName = file.Value();
                        }

                        Console.WriteLine("Salvando respostas em arquivo...");
                        File.WriteAllText(fileName, builder.ToString());

                        Console.WriteLine($"Respostas salvas no arquivo \"{fileName}\" com sucesso!");
                    }
                    else
                    {
                        Console.WriteLine(result.Message);
                    }
                });
            });
        }

        private static void BuildDetailedReportCommand(CommandLineApplication app)
        {
            app.Command("report", (command) =>
            {
                var key = command.Option("-k|--key", "Chave de envio da API.", CommandOptionType.SingleValue);
                var startDate = command.Option("-s|--startdate", "Data inicial do relatório de respostas.", CommandOptionType.SingleValue);
                var endDate = command.Option("-e|--enddate", "Data final do relatório de respostas.", CommandOptionType.SingleValue);
                var file = command.Option("-f|--file", "Nome do arquivo que as respostas serão salvas", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    Console.WriteLine("Buscando relatório...");
                    var textMessageService = new TextMessageService(key.Value());
                    var result = textMessageService.GetDetailedReport(DateTime.Parse(startDate.Value()), DateTime.Parse(endDate.Value()), Sdk.Core.Resources.DeliveryStatus.All);

                    if (result.Success)
                    {
                        Console.WriteLine("Processando relatório...");
                        var builder = new StringBuilder();
                        builder.AppendLine($"SENDER;RECEIVER;CONTENT;SYSTEM MESSAGE;STATUS;REQUEST DATE;SCHEDULE DATE");
                        foreach (var message in result.Object)
                        {
                            builder.AppendLine($"{message.Sender};{message.Receiver};{message.Content};{message.SystemMessage};{message.Status};{message.RequestDate:yyyy-MM-dd HH:mm:ss};{(message.ScheduleDate == null ? "Sem Agendamento" : message.ScheduleDate?.ToString("yyyy-MM-dd HH:mm:ss"))}");
                        }

                        string fileName = "report.csv";
                        if (file.HasValue())
                        {
                            fileName = file.Value();
                        }

                        Console.WriteLine("Salvando relatório em arquivo...");
                        File.WriteAllText(fileName, builder.ToString());

                        Console.WriteLine($"Relatório salvo no arquivo \"{fileName}\" com sucesso!");
                    }
                    else
                    {
                        Console.WriteLine(result.Message);
                    }
                });
            });
        }
    }
}