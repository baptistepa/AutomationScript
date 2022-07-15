namespace Script
{
	using System;
	using System.Collections.Generic;
	using AdaptiveCards;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (Exception ex)
			{
				engine.AddError(ex.Message);
			}
		}

		private void RunSafe(Engine engine)
		{
			ResourceManagerHelper resourceManager = new ResourceManagerHelper();
			resourceManager.RequestResponseEvent += (sender, e) => e.responseMessage = Engine.SLNet.SendSingleResponseMessage(e.requestMessage);

			List<BookingInfo> bookings = new List<BookingInfo>();
			foreach (var reservation in resourceManager.GetReservationInstances(ReservationInstanceExposers.Status.Equal((int)ReservationStatus.Ongoing)))
			{
				var bookingInfo = new BookingInfo
				{
					Id = Convert.ToString(reservation.ID),
					Name = reservation.Name,
					Start = reservation.Start,
					End = reservation.End,
				};

				if (reservation.Properties.Dictionary.TryGetValue("JobCustomer", out object customerName))
				{
					bookingInfo.CustomerName = Convert.ToString(customerName);
				}

				bookings.Add(bookingInfo);
			}

			var bookingFactSets = new List<AdaptiveFactSet>();

			foreach (var booking in bookings)
			{
				var infoFacts = new List<AdaptiveFact>();

				infoFacts.Add(new AdaptiveFact("Id:", booking.Id));
				infoFacts.Add(new AdaptiveFact("Name:", booking.Name));
				infoFacts.Add(new AdaptiveFact("Start:", booking.Start.ToString()));
				infoFacts.Add(new AdaptiveFact("End:", booking.End.ToString()));

				if (!String.IsNullOrEmpty(booking.CustomerName))
				{
					infoFacts.Add(new AdaptiveFact("Customer Name:", booking.CustomerName));
				}

				bookingFactSets.Add(new AdaptiveFactSet()
				{
					Facts = infoFacts
				});

			}

			engine.AddScriptOutput("AdaptiveCard", JsonConvert.SerializeObject(bookingFactSets));
		}
	}
}
//---------------------------------
// Script\Objects\BookingInfo.cs
//---------------------------------
namespace Script
{
	using System;

	internal class BookingInfo
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public DateTime Start { get; set; }

		public DateTime End { get; set; }

		public string CustomerName { get; set; }
	}
}

//---------------------------------
// Script\Objects\ErrorMessage.cs
//---------------------------------
namespace Script
{
	public class ErrorMessage
	{
		public string Message { get; set; }
	}
}

//---------------------------------
// Script\Objects\ResultMessage.cs
//---------------------------------
namespace Script
{
	internal class ResultMessage
	{

	}
}