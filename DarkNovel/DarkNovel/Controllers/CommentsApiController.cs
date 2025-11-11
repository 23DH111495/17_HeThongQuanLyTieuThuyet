using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DarkNovel.Data;
using DarkNovel.Models;
using DarkNovel.Models.ApiModels;

namespace WebNovel.Controllers.Api
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsApiController : ControllerBase
    {
        private readonly DarkNovelContext _db;

        public CommentsApiController(DarkNovelContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("novel/{novelId:int}")]
        public IActionResult GetNovelComments(int novelId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _db.Comments
                    .Where(c => c.NovelId == novelId && c.ParentCommentId == null && c.IsApproved)
                    .OrderByDescending(c => c.CreatedAt);

                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var comments = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.User)
                    .ToList();

                var commentIds = comments.Select(c => c.Id).ToList();
                var replies = _db.Comments
                    .Where(c => c.ParentCommentId.HasValue && commentIds.Contains(c.ParentCommentId.Value) && c.IsApproved)
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                var result = comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User?.Username ?? "Anonymous",
                    Content = c.Content ?? "",
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    HasImage = c.CommentImage != null && c.CommentImage.Length > 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Replies = replies.Where(r => r.ParentCommentId == c.Id).Select(r => new CommentDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        Username = r.User?.Username ?? "Anonymous",
                        Content = r.Content ?? "",
                        LikeCount = r.LikeCount,
                        DislikeCount = r.DislikeCount,
                        HasImage = r.CommentImage != null && r.CommentImage.Length > 0,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Replies = new List<CommentDto>()
                    }).ToList()
                }).ToList();

                return Ok(new PaginatedApiResponse<List<CommentDto>>
                {
                    Success = true,
                    Data = result,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving comments: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("chapter/{chapterId:int}")]
        public IActionResult GetChapterComments(int chapterId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _db.Comments
                    .Where(c => c.ChapterId == chapterId && c.ParentCommentId == null && c.IsApproved)
                    .OrderByDescending(c => c.CreatedAt);

                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var comments = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.User)
                    .ToList();

                var commentIds = comments.Select(c => c.Id).ToList();
                var replies = _db.Comments
                    .Where(c => c.ParentCommentId.HasValue && commentIds.Contains(c.ParentCommentId.Value) && c.IsApproved)
                    .Include(c => c.User)
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                var result = comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User?.Username ?? "Anonymous",
                    Content = c.Content ?? "",
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    HasImage = c.CommentImage != null && c.CommentImage.Length > 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Replies = replies.Where(r => r.ParentCommentId == c.Id).Select(r => new CommentDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        Username = r.User?.Username ?? "Anonymous",
                        Content = r.Content ?? "",
                        LikeCount = r.LikeCount,
                        DislikeCount = r.DislikeCount,
                        HasImage = r.CommentImage != null && r.CommentImage.Length > 0,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Replies = new List<CommentDto>()
                    }).ToList()
                }).ToList();

                return Ok(new PaginatedApiResponse<List<CommentDto>>
                {
                    Success = true,
                    Data = result,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving comments: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateComment([FromForm] CreateCommentRequest request)
        {
            if (request.UserId <= 0)
            {
                return Unauthorized(new { message = "User must be logged in to comment" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Comment content is required" });
            }

            if (!request.NovelId.HasValue && !request.ChapterId.HasValue)
            {
                return BadRequest(new { message = "Either NovelId or ChapterId must be provided" });
            }

            try
            {
                var comment = new Comment
                {
                    UserId = request.UserId,
                    NovelId = request.NovelId,
                    ChapterId = request.ChapterId,
                    ParentCommentId = request.ParentCommentId,
                    Content = request.Content,
                    IsApproved = true,
                    ModerationStatus = "Approved",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                if (request.Image != null && request.Image.Length > 0)
                {
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        request.Image.CopyTo(memoryStream);
                        comment.CommentImage = memoryStream.ToArray();
                        comment.CommentImageContentType = request.Image.ContentType;
                        comment.CommentImageFileName = request.Image.FileName;
                    }
                }

                _db.Comments.Add(comment);
                _db.SaveChanges();

                var user = _db.Users.FirstOrDefault(u => u.Id == request.UserId);

                return Ok(new ApiResponse<CommentDto>
                {
                    Success = true,
                    Message = "Comment posted successfully",
                    Data = new CommentDto
                    {
                        Id = comment.Id,
                        UserId = comment.UserId,
                        Username = user?.Username ?? "Anonymous",
                        Content = comment.Content ?? "",
                        LikeCount = 0,
                        DislikeCount = 0,
                        HasImage = comment.CommentImage != null && comment.CommentImage.Length > 0,
                        CreatedAt = comment.CreatedAt,
                        UpdatedAt = comment.UpdatedAt,
                        Replies = new List<CommentDto>()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating comment: " + ex.Message });
            }
        }

        [HttpPut]
        [Route("{commentId:int}")]
        public IActionResult UpdateComment(int commentId, [FromForm] UpdateCommentRequest request)
        {
            if (request.UserId <= 0)
            {
                return Unauthorized(new { message = "User must be logged in" });
            }

            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);

                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                if (comment.UserId != request.UserId)
                {
                    return Forbid();
                }

                if (!string.IsNullOrWhiteSpace(request.Content))
                {
                    comment.Content = request.Content;
                }

                if (request.Image != null && request.Image.Length > 0)
                {
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        request.Image.CopyTo(memoryStream);
                        comment.CommentImage = memoryStream.ToArray();
                        comment.CommentImageContentType = request.Image.ContentType;
                        comment.CommentImageFileName = request.Image.FileName;
                    }
                }
                else if (request.RemoveImage)
                {
                    comment.CommentImage = null;
                    comment.CommentImageContentType = null;
                    comment.CommentImageFileName = null;
                }

                comment.UpdatedAt = DateTime.Now;
                _db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Comment updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating comment: " + ex.Message });
            }
        }

        [HttpDelete]
        [Route("{commentId:int}")]
        public IActionResult DeleteComment(int commentId, [FromBody] DeleteCommentRequest request)
        {
            if (request.UserId <= 0)
            {
                return Unauthorized(new { message = "User must be logged in" });
            }

            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);

                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                if (comment.UserId != request.UserId)
                {
                    return Forbid();
                }

                var replies = _db.Comments.Where(c => c.ParentCommentId == commentId).ToList();
                _db.Comments.RemoveRange(replies);

                _db.Comments.Remove(comment);
                _db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Comment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting comment: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("{commentId:int}/like")]
        public IActionResult LikeComment(int commentId, [FromBody] VoteCommentRequest request)
        {
            if (request.UserId <= 0)
            {
                return Unauthorized(new { message = "User must be logged in" });
            }

            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);

                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                var likedUsers = string.IsNullOrEmpty(comment.LikedUserIds)
                    ? new List<string>()
                    : comment.LikedUserIds.Split(',').ToList();
                var dislikedUsers = string.IsNullOrEmpty(comment.DislikedUserIds)
                    ? new List<string>()
                    : comment.DislikedUserIds.Split(',').ToList();

                var userIdStr = request.UserId.ToString();

                if (dislikedUsers.Contains(userIdStr))
                {
                    dislikedUsers.Remove(userIdStr);
                    comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
                }

                if (likedUsers.Contains(userIdStr))
                {
                    likedUsers.Remove(userIdStr);
                    comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                }
                else
                {
                    likedUsers.Add(userIdStr);
                    comment.LikeCount++;
                }

                comment.LikedUserIds = string.Join(",", likedUsers.Where(u => !string.IsNullOrEmpty(u)));
                comment.DislikedUserIds = string.Join(",", dislikedUsers.Where(u => !string.IsNullOrEmpty(u)));

                _db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Like updated",
                    Data = new
                    {
                        LikeCount = comment.LikeCount,
                        DislikeCount = comment.DislikeCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error liking comment: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("{commentId:int}/dislike")]
        public IActionResult DislikeComment(int commentId, [FromBody] VoteCommentRequest request)
        {
            if (request.UserId <= 0)
            {
                return Unauthorized(new { message = "User must be logged in" });
            }

            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);

                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }

                var likedUsers = string.IsNullOrEmpty(comment.LikedUserIds)
                    ? new List<string>()
                    : comment.LikedUserIds.Split(',').ToList();
                var dislikedUsers = string.IsNullOrEmpty(comment.DislikedUserIds)
                    ? new List<string>()
                    : comment.DislikedUserIds.Split(',').ToList();

                var userIdStr = request.UserId.ToString();

                if (likedUsers.Contains(userIdStr))
                {
                    likedUsers.Remove(userIdStr);
                    comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                }

                if (dislikedUsers.Contains(userIdStr))
                {
                    dislikedUsers.Remove(userIdStr);
                    comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
                }
                else
                {
                    dislikedUsers.Add(userIdStr);
                    comment.DislikeCount++;
                }

                comment.LikedUserIds = string.Join(",", likedUsers.Where(u => !string.IsNullOrEmpty(u)));
                comment.DislikedUserIds = string.Join(",", dislikedUsers.Where(u => !string.IsNullOrEmpty(u)));

                _db.SaveChanges();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Dislike updated",
                    Data = new
                    {
                        LikeCount = comment.LikeCount,
                        DislikeCount = comment.DislikeCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error disliking comment: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("{commentId:int}/image")]
        public IActionResult GetCommentImage(int commentId)
        {
            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);

                if (comment == null || comment.CommentImage == null || comment.CommentImage.Length == 0)
                {
                    return NotFound();
                }

                var contentType = comment.CommentImageContentType ?? "image/jpeg";
                return File(comment.CommentImage, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving image: " + ex.Message });
            }
        }
    }

    public class CreateCommentRequest
    {
        public int UserId { get; set; }
        public int? NovelId { get; set; }
        public int? ChapterId { get; set; }
        public int? ParentCommentId { get; set; }
        public string? Content { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateCommentRequest
    {
        public int UserId { get; set; }
        public string? Content { get; set; }
        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; }
    }

    public class DeleteCommentRequest
    {
        public int UserId { get; set; }
    }

    public class VoteCommentRequest
    {
        public int UserId { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Content { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool HasImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CommentDto>? Replies { get; set; }
    }
}