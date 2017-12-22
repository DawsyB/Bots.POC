using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bot.Multipurpose.Forms
{
	public class FeedbackForm
	{
		[Required]
		public string FullName { get; set; }

		[Required]
		public DateTime? DOB { get; set; }

		[Range(1, 60)]
		public int? YearsOfExpierence { get; set; }

		[Required]
		[EmailAddress]
		public String Email { get; set; }

		[Required]
		[Phone]
		public String Phone { get; set; }

		[Required]
		public string Comments { get; set; }

		public static FeedbackForm Parse(dynamic f)
		{
			try
			{
				return new FeedbackForm
				{
					FullName = f.FullName.ToString(),
					DOB = DateTime.Parse(f.DOB.ToString()),
					YearsOfExpierence = int.Parse(f.YearsOfExpierence.ToString()),
					Email = f.Email.ToString(),
					Phone = f.Phone.ToString(),
					Comments = f.Comments.ToString()

				};
			}
			catch(Exception ex)
			{
				throw new InvalidCastException("FeedbackForm could not be read");
			}
		}
	}
}