using Sammlerplattform.Data;
using Sammlerplattform.Models.ImprovementSuggestions;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessImprovementSuggestions
    {
        List<Topic> GetWithPredicate(TopicSearchParameterModel searchParameterModel, int count = 20);
        (int statusCode, string StatusMessage, int ForumTopicId) Insert(string title, string content, string authorId);
        (int statusCode, string StatusMessage, int ForumTopicId) Update(int topicId, string title, string content);
        (int statusCode, string StatusMessage) Delete(int topicId);
    }

    public class ImprovementSuggestionsProcessor(IUnitOfWork unitOfWork
        , ITrackEventsText trackEvents) : IProcessImprovementSuggestions
    {
        public List<Topic> GetWithPredicate(TopicSearchParameterModel searchParameterModel, int count = 20)
        {
            IEnumerable<Topic> topics = unitOfWork.ForumTopicRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Topic>(searchParameterModel), includeProperties: "Author,VoteList");

            return [.. topics];
        }

        public (int statusCode, string StatusMessage, int ForumTopicId) Insert(string title, string content, string authorId)
        {
            (bool flowControl, (int statusCode, string StatusMessage, int ForumTopicId) value) = ParameterCheck(title, content);
            if (!flowControl)
            {
                return value;
            }

            try
            {
                Topic newTopic = new()
                {
                    Title = title,
                    Content = content,
                    UserId = authorId,
                    CreatedAt = DateTime.UtcNow
                };
                newTopic = unitOfWork.ForumTopicRepository.Insert(newTopic);
                unitOfWork.Save();

                return (200, "Success_ImprovementSuggestion_Created", newTopic.Id);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ImprovementSuggestionsProcessor.Insert");
                return (500, "Error_Unknown", 0);
            }
        }

        private static (bool flowControl, (int statusCode, string StatusMessage, int ForumTopicId) value) ParameterCheck(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title))
                return (flowControl: false, value: (400, "Error_Title_Empty", 0));
            if (string.IsNullOrWhiteSpace(content))
                return (flowControl: false, value: (400, "Error_Content_Empty", 0));
            return (flowControl: true, value: default);
        }

        public (int statusCode, string StatusMessage, int ForumTopicId) Update(int topicId, string title, string content)
        {
            (bool flowControl, (int statusCode, string StatusMessage, int ForumTopicId) value) = ParameterCheck(title, content);
            if (!flowControl)
            {
                return value;
            }

            Topic? existingTopic = GetWithPredicate(new TopicSearchParameterModel { Id = [topicId] }).FirstOrDefault();
            if (existingTopic == null)
            {
                return (404, "Error_ImprovementSuggestion_NotFound", 0);
            }

            try
            {
                existingTopic.Title = title;
                existingTopic.Content = content;
                existingTopic.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Save();
                return (200, "Success_ImprovementSuggestion_Updated", existingTopic.Id);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ImprovementSuggestionsProcessor.Update");
                return (500, "Error_Unknown", 0);
            }
        }

        public (int statusCode, string StatusMessage) Delete(int topicId)
        {
            Topic? existingTopic = GetWithPredicate(new TopicSearchParameterModel { Id = [topicId] }).FirstOrDefault();
            if (existingTopic == null)
            {
                return (404, "Error_ImprovementSuggestion_NotFound");
            }
            try
            {
                unitOfWork.ForumTopicRepository.Delete(existingTopic);
                unitOfWork.Save();
                return (200, "Success_ImprovementSuggestion_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "ImprovementSuggestionsProcessor.Delete");
                return (500, "Error_Unknown");
            }
        }
    }
}
