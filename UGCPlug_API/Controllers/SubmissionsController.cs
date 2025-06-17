using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using UGCPlug_API.Models;

namespace UGCPlug_API.Controllers
{
    // This Web API controller handles user-generated content submissions.
    // It supports full CRUD functionality: create, read, update, delete.
    public class SubmissionsController : ApiController
    {
        // Connect to the Entity Framework DB context for UGC submissions
        private UGCPlug_API_DB_ConnectionString db = new UGCPlug_API_DB_ConnectionString();

        // GET: api/Submissions?businessId=zaff-papers
        // Return all submissions for a specific business ID, newest first
        [HttpGet]
        [Route("api/Submissions")]
        public IEnumerable<Submission> Get(string businessId)
        {
            return db.Submissions
                     .Where(s => s.BusinessId == businessId)
                     .OrderByDescending(s => s.DateSubmitted)
                     .ToList();
        }

        // POST: api/Submissions
        // Accept a UGC submission (including file upload) from a public form
        [HttpPost]
        [Route("api/Submissions")]
        public IHttpActionResult Post()
        {
            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Files.Count == 0)
                return BadRequest("No file uploaded.");

            try
            {
                // Save the uploaded file to the server
                var file = httpRequest.Files[0];
                var fileName = Guid.NewGuid() + "_" + file.FileName;
                var savePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + fileName);
                file.SaveAs(savePath);

                // Extract form values and create a new Submission object
                var submission = new Submission
                {
                    FirstName = httpRequest.Form["FirstName"],
                    LastName = httpRequest.Form["LastName"],
                    Email = httpRequest.Form["Email"],
                    City = httpRequest.Form["City"],
                    Country = httpRequest.Form["Country"],
                    DateOfBirth = ParseNullableDate(httpRequest.Form["DateOfBirth"]),
                    DateTaken = ParseNullableDate(httpRequest.Form["DateTaken"]),
                    DateSubmitted = DateTime.Now,
                    FileUrl = "/UploadedFiles/" + fileName,
                    ConsentGiven = httpRequest.Form["ConsentGiven"] == "true" || httpRequest.Form["ConsentGiven"] == "on",
                    BusinessId = httpRequest.Form["BusinessId"]
                };

                // Validate required fields and consent
                if (string.IsNullOrWhiteSpace(submission.FirstName) ||
                    string.IsNullOrWhiteSpace(submission.LastName) ||
                    string.IsNullOrWhiteSpace(submission.Email) ||
                    string.IsNullOrWhiteSpace(submission.BusinessId) ||
                    !submission.ConsentGiven)
                {
                    return BadRequest("Missing required fields or consent not given.");
                }

                // Prevent future-dated fields
                if (submission.DateOfBirth > DateTime.Today)
                    return BadRequest("Date of birth cannot be in the future.");

                if (submission.DateTaken > DateTime.Today)
                    return BadRequest("Date taken cannot be in the future.");

                // Block unrealistically old dates
                if (submission.DateOfBirth < new DateTime(1900, 1, 1))
                    return BadRequest("Please enter a valid date of birth.");

                // Save submission to database
                db.Submissions.Add(submission);
                db.SaveChanges();

                return Ok(new { message = "Submission received." });
            }
            catch (Exception ex)
            {
                // Return error message if anything fails
                return BadRequest("Server error: " + ex.Message);
            }
        }

        // Helper method to parse optional date strings into nullable DateTime
        private DateTime? ParseNullableDate(string input)
        {
            if (DateTime.TryParse(input, out DateTime result))
                return result;
            return null;
        }

        // PUT: api/Submissions/5
        // Update an existing submission (used by business via dashboard)
        [HttpPut]
        [Route("api/Submissions/{id}")]
        public IHttpActionResult PutSubmission(int id, Submission submission)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != submission.Id)
                return BadRequest("ID mismatch");

            var existing = db.Submissions.Find(id);
            if (existing == null)
                return NotFound();

            // Apply updated values and save
            db.Entry(existing).CurrentValues.SetValues(submission);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Submissions/5
        // Remove a submission from the database by ID
        [HttpDelete]
        [Route("api/Submissions/{id}")]
        public IHttpActionResult DeleteSubmission(int id)
        {
            var submission = db.Submissions.Find(id);
            if (submission == null)
                return NotFound();

            db.Submissions.Remove(submission);
            db.SaveChanges();

            return Ok();
        }

        // GET: api/Submissions/5
        // Retrieve a single submission by ID (used for edit/delete in dashboard)
        [HttpGet]
        [Route("api/Submissions/{id}")]
        public IHttpActionResult GetSubmission(int id)
        {
            var submission = db.Submissions.Find(id);
            if (submission == null)
                return NotFound();

            return Ok(submission);
        }
    }
}
