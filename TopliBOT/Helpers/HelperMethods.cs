using Discord;
using Discord.WebSocket;

namespace TopliBOT.Helpers
{
    public class HelperMethods
    {
        public EmbedBuilder BuildEmbed(string footerText, string title, string trackTitle, string trackUrl, string thumbUrl, SocketUser user)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder
               .WithFooter(footer => footer.WithIconUrl(user.GetAvatarUrl()).WithText(footerText))
               .WithColor(Color.Blue)
               .WithTitle(title)
               .WithDescription("[" + trackTitle + "]" + "(" + trackUrl + ")")
               .WithThumbnailUrl(thumbUrl)
               .WithCurrentTimestamp();

            return embedBuilder;
        }
    }
}
