﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using Tutor.api.Services;
using Tutor.Api.Identity;
using Tutor.Api.Models;

namespace Tutor.Api.Controllers
{
    [Route("Questions")]
    [ApiController]
    public class QuestionController : Controller
    {
        private readonly ILogger<QuestionController> _logger;
        private readonly Services.EmailService _service;
        private readonly DatabaseService _database;
        public QuestionController(ILogger<QuestionController> logger, Services.EmailService service, DatabaseService database)
        {
            _logger = logger;
            _service = service;
            _database = database;   
        }
        [HttpGet("GetQuestions")]
        [Authorize(Roles = "Tutor, Admin")]
        public IEnumerable<Question>? GetQuestions(String? classCode, String? topic)
        {
            return _database.GetQuestions(classCode, topic);

        }

        [HttpPost("AskQuestion")]
        [Authorize(Roles = Roles.Student)]
        public String AddQuestion(String studentUsername, String classCode, String topic, String question) 
        { 
            if (string.IsNullOrWhiteSpace(studentUsername)) 
            if (string.IsNullOrWhiteSpace(classCode)) { return "Error, classCode cannot be blank."; }
            if (string.IsNullOrWhiteSpace(topic)) { return "Error, topic cannot be blank."; }
            if (string.IsNullOrWhiteSpace(question)) { return "Error, question cannot be blank."; }

            int studentId = _database.getStudentId(studentUsername);
            int? checkId = _database.getClassId(classCode);
            if (!checkId.HasValue) { return "Error, class does not exist."; }
            int classId = (int)checkId;
            int topicId = _database.getTopicId(topic);

            Question q = new()
            {
                QuestionId = Guid.NewGuid(),
                StudentId = studentId,
                ClassId = classId,
                TopicId = topicId,
                Question1 = question
            };

            try
            {
                _database.addQuestion(q);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "Error, failed to save question.";
            }

            if(_service.SendQuestionConfirmation(q.QuestionId, studentUsername))
            {
                return "Question " + q.QuestionId + " has successfully beem submitted.";
            }
            else
            {
                return "Question " + q.QuestionId + " has successfully beem submitted, but email confirmation has not been sent due to an error.";
            }
        }

        [HttpPost("AnswerQuestion")]
        [Authorize(Roles = Roles.Tutor)]
        public String AnswerQuestion(Guid questionID, String tutorUsername, String answer)
        {
            if(tutorUsername == null) { return "Error, tutorUsername cannot be blank."; }
            if(answer == null) { return "Error, answer cannot be blank."; }

            Question q = _database.GetQuestion(questionID);

            if(q == null) { return "Error, Question not found."; }

            int tutorId = _database.getTutorId(tutorUsername);

            AnsweredQuestion a = new()
            {
                QuestionId = q.QuestionId,
                StudentId = q.StudentId,
                TutorId = tutorId,
                ClassId = q.ClassId,
                TopicId = q.TopicId,
                Question = q.Question1,
                Response = answer
            };

            Boolean result = _database.AnswerQuestion(a);
            if(result) { return "Successfully answered question."; }
            else { return "Error, could not save answer."; }
        }

    }

}
