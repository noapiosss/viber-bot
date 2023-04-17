using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Models;
using Domain.Queries;
using MediatR;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;
using Web.Services.Interfaces;
using Web.Services.Models;
using static Viber.Bot.NetCore.Models.ViberMessage;

namespace Web.Services
{
    public class ViberService : IViberService
    {
        private readonly IViberBotApi _viberBotApi;
        private readonly IMediator _mediator;
        private readonly IUsersInfo _usersInfo;

        public ViberService(IViberBotApi viberBotApi, IMediator mediator, IUsersInfo usersNavigation)
        {
            _viberBotApi = viberBotApi;
            _mediator = mediator;
            _usersInfo = usersNavigation;
        }

        public async Task HandleMessage(ViberCallbackData update, CancellationToken cancellationToken = default)
        {
            if (update.Event is ViberEventType.Subscribed)
            {
                if (_usersInfo.UserInAnyStage(update.User.Id))
                {
                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateSubscriptionMessage(update, "Wellcome back!"));
                    _usersInfo.SetStage(update.User.Id, Stage.MainMenu);
                }
                else
                {
                    _usersInfo.SetStage(update.User.Id, Stage.Authorization);

                    string responseMessage = "Wellcome!\nTo start enter your IMEI";
                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateSubscriptionMessage(update, responseMessage));
                    return;
                }
            }

            if (!_usersInfo.UserInAnyStage(update.Sender.Id))
            {
                _usersInfo.SetStage(update.Sender.Id, Stage.Authorization);

                string responseMessage = "Enter your IMEI";
                _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateTextMessage(update, responseMessage));
                return;
            }

            TextMessage message;

            if (update.Message.Type == ViberMessageType.Text)
            {
                message = update.Message as TextMessage;
            }
            else
            {
                return;
            }

            if (_usersInfo.GetStage(update.Sender.Id) == Stage.Authorization)
            {
                if (!IsImei(message.Text))
                {
                    string responseMessage = "Invalid IMEI, try again";
                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateTextMessage(update, responseMessage));
                    return;
                }
                else
                {
                    if (await TryAddImei(update, message.Text, cancellationToken))
                    {
                        _usersInfo.SetStage(update.Sender.Id, Stage.MainMenu);
                        await ShowMainMenu(update, cancellationToken);
                    }
                    return;
                }
            }

            if (_usersInfo.GetStage(update.Sender.Id) == Stage.MainMenu)
            {
                if (message.Text.Equals("/top10walks", StringComparison.Ordinal))
                {
                    _ = _usersInfo.TryGetImei(update.Sender.Id, out string imei);
                    GetWalksByImeiQuery query = new() { Imei = imei };
                    GetWalksByImeiQueryResult result = await _mediator.Send(query, cancellationToken);


                    _usersInfo.SetStage(update.Sender.Id, Stage.Top10Walks);

                    if (result == null)
                    {
                        _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardTop10WalksMessage(update, "No any walk yet"));
                        return;
                    }

                    StringBuilder sb = new();
                    int i = 1;
                    foreach (Walk walk in result.Walks.Take(10))
                    {
                        _ = sb.AppendLine($"Top {i}");
                        _ = sb.AppendLine($"Distance: {Math.Round(walk.Distance, 2)} km");
                        _ = sb.AppendLine($"Duration: {Math.Round(walk.Duration.TotalMinutes, 2)} min");
                        _ = sb.AppendLine();
                        ++i;
                    }

                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardTop10WalksMessage(update, sb.ToString()));
                    return;
                }

                if (message.Text.Equals("/changeIMEI", StringComparison.Ordinal))
                {
                    _usersInfo.SetStage(update.Sender.Id, Stage.ChangeImei);
                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardChangeImeiMessage(update, "Eneter new IMEI"));
                    return;
                }
            }

            if (_usersInfo.GetStage(update.Sender.Id) == Stage.ChangeImei)
            {
                if (message.Text.Equals("/main_menu", StringComparison.Ordinal))
                {
                    _usersInfo.SetStage(update.Sender.Id, Stage.MainMenu);
                    await ShowMainMenu(update, cancellationToken);
                    return;
                }

                if (!IsImei(message.Text))
                {
                    _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardChangeImeiMessage(update, "Invalid IMEI"));
                    return;
                }
                else
                {
                    if (await TrySetImei(update, message.Text, cancellationToken))
                    {
                        _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardChangeImeiMessage(update, "IMEI was successfully changed"));
                        return;
                    }
                }
            }

            if (_usersInfo.GetStage(update.Sender.Id) == Stage.Top10Walks)
            {
                if (message.Text.Equals("/main_menu", StringComparison.Ordinal))
                {
                    _usersInfo.SetStage(update.Sender.Id, Stage.MainMenu);
                    await ShowMainMenu(update, cancellationToken);
                    return;
                }
            }

            return;
        }


        private static TextMessage CreateTextMessage(ViberCallbackData update, string text)
        {
            return new()
            {
                Receiver = update.Sender == null ? update.User.Id : update.Sender.Id,
                Sender = new ViberUser.User()
                {
                    Name = "walktrackerbot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                Text = text
            };
        }

        private static TextMessage CreateSubscriptionMessage(ViberCallbackData update, string text)
        {
            return new()
            {
                Receiver = update.User.Id,
                Sender = new ViberUser.User()
                {
                    Name = "walktrackerbot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                Text = text
            };
        }

        private static KeyboardMessage CreateKeyboardMainMenuMessage(ViberCallbackData update, string text)
        {
            ViberKeyboardButton getTop10Button = new()
            {
                Text = "Top 10 walks",
                ActionType = "reply",
                ActionBody = "/top10walks",
                Columns = 3
            };

            ViberKeyboardButton changeImeiButton = new()
            {
                Text = "Change IMEI",
                ActionType = "reply",
                ActionBody = "/changeIMEI",
                Columns = 3
            };

            ViberKeyboard keyboard = new()
            {
                Buttons = new List<ViberKeyboardButton>() { getTop10Button, changeImeiButton },
                ButtonsGroupColumns = 6
            };

            return new()
            {
                Receiver = update.Sender.Id,
                Sender = new ViberUser.User()
                {
                    Name = "walktrackerbot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                Text = text,
                Keyboard = keyboard
            };
        }

        private static KeyboardMessage CreateKeyboardTop10WalksMessage(ViberCallbackData update, string text)
        {
            ViberKeyboardButton backButton = new()
            {
                Text = "Back",
                ActionType = "reply",
                ActionBody = "/main_menu"
            };

            ViberKeyboard keyboard = new()
            {
                Buttons = new List<ViberKeyboardButton>() { backButton }
            };

            return new()
            {
                Receiver = update.Sender.Id,
                Sender = new ViberUser.User()
                {
                    Name = "walktrackerbot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                Text = text,
                Keyboard = keyboard
            };
        }

        private static KeyboardMessage CreateKeyboardChangeImeiMessage(ViberCallbackData update, string text)
        {
            ViberKeyboardButton backButton = new()
            {
                Text = "Back",
                ActionType = "reply",
                ActionBody = "/main_menu"
            };

            ViberKeyboard keyboard = new()
            {
                Buttons = new List<ViberKeyboardButton>() { backButton }
            };

            return new()
            {
                Receiver = update.Sender.Id,
                Sender = new ViberUser.User()
                {
                    Name = "walktrackerbot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                Text = text,
                Keyboard = keyboard
            };
        }

        private async Task<bool> TryAddImei(ViberCallbackData update, string imei, CancellationToken cancellationToken)
        {
            ImeiExistsQuery imeiExistsQuery = new() { Imei = imei };
            ImeiExistsQueryResult imeiExistsQueryResult = await _mediator.Send(imeiExistsQuery, cancellationToken);

            string responseMessage;
            if (!imeiExistsQueryResult.EmeiExists)
            {
                responseMessage = "There is no such IMEI";
                _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateTextMessage(update, responseMessage));
                return false;
            }

            return _usersInfo.TryAddImei(update.Sender.Id, imei);
        }

        private async Task<bool> TrySetImei(ViberCallbackData update, string imei, CancellationToken cancellationToken)
        {
            ImeiExistsQuery imeiExistsQuery = new() { Imei = imei };
            ImeiExistsQueryResult imeiExistsQueryResult = await _mediator.Send(imeiExistsQuery, cancellationToken);

            string responseMessage;
            if (!imeiExistsQueryResult.EmeiExists)
            {
                responseMessage = "There is no such IMEI";
                _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardChangeImeiMessage(update, responseMessage));
                return false;
            }

            _usersInfo.SetImei(update.Sender.Id, imei);
            return true;
        }

        private async Task ShowMainMenu(ViberCallbackData update, CancellationToken cancellationToken)
        {
            _ = _usersInfo.TryGetImei(update.Sender.Id, out string imei);
            GetWalksByImeiQuery getWalksByImeiQuery = new() { Imei = imei };
            GetWalksByImeiQueryResult getWalksByImeiQueryResult = await _mediator.Send(getWalksByImeiQuery, cancellationToken);

            if (getWalksByImeiQueryResult == null)
            {
                _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateTextMessage(update, "No any walk yet"));
                return;
            }

            double totalDistance = 0;
            double totalDuration = 0;
            foreach (Walk walk in getWalksByImeiQueryResult.Walks)
            {
                totalDistance += walk.Distance;
                totalDuration += walk.Duration.TotalMinutes;
            }

            string responseMessage = $"Walks count: {getWalksByImeiQueryResult.Walks.Count}\n" +
                $"Total distanse: {Math.Round(totalDistance, 2)} kilometers\n" +
                $"Total duration: {Math.Round(totalDuration, 2)} minutes";
            _ = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(CreateKeyboardMainMenuMessage(update, responseMessage));
        }

        private static bool IsImei(string imei)
        {
            if (imei.Length != 15)
            {
                return false;
            }

            foreach (char c in imei)
            {
                if (!char.IsAsciiDigit(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}