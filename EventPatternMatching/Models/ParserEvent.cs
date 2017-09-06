using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace EventPatternMatching.Models
{
    /// <summary>
    /// This Class is third Party class which is implemented the IEventCounter Interface
    /// and calculate all fault which is happend.
    /// </summary>
    public class ParserEvent : IEventCounter
    {
        #region GetEventCount
        /// <summary>
        /// Gets the event count.
        /// </summary>
        /// <returns>The event count.</returns>
        /// <param name="deviceID">Device identifier.</param>
        public int GetEventCount(string deviceID)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ParseEvents
        /// <summary>
        /// Parses the events.
        /// </summary>
        /// <param name="deviceID">Device identifier.</param>
        /// <param name="eventLog">Event log.</param>
        public void ParseEvents(string deviceID, StreamReader eventLog)
        {
            var count = 0;
            //read eventlog and convert it to a List
            //With that list now we can caount fault.
            var objList = GenerateList(eventLog);
         
            //At First Step we should find index of a stage which is propbaly fault was happend on that stage
            //Acording the assumption that Stage would be at 3 and time would be longer that 5 minutes
            int indexOfValue = objList.FindIndex(a => a.StageNumber==3 && a.StageDiffTime>=5);

			var result = Enumerable.Range(0, objList.Count)
                                   .Where(a => objList[a].StageNumber == 3 && objList[a].StageDiffTime >= 5)
			 .ToList();
            if(result.Count()>1)
            //store this count with DeviceId in Database
            count = CalculateFault(objList,result, result[0], true,count);


			// save on db 
			EventPatternMatchingContext _context= new EventPatternMatchingContext();
          
			_context.Add(new Models.EventPatternMatching
			{
				DeviceId = deviceID,
				Count = count
			});
                 _context.SaveChangesAsync();
           
        }
        #endregion

        #region Calculate Count
        /// <summary>
        /// this recursive function is used to calculate all fault whcih happen in sequence of stages
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="stagelist">Stagelist.</param>
        /// <param name="index">Index.</param>
        /// <param name="counter">Counter.</param>
        public int CalculateFault(List<StageEntity> stagelist, List<int> SuspiciousList, int index, bool isStartLoop, int counter)
        {

            var tmpIndex = index++;
            //check for the rest of theformula. next stage should be 2 and
            //the last stage should be 0 to finish iteration at each step
            if (stagelist[tmpIndex].StageNumber == 2 && isStartLoop)
            {
                isStartLoop = false;
                tmpIndex++;
                while (stagelist[tmpIndex].StageNumber != 0 && tmpIndex < stagelist.Count)
                {
                    tmpIndex++;
                    continue;
                }

                if (tmpIndex < stagelist.Count)
                    counter++;
            }

            if (tmpIndex++ < stagelist.Count)
            {
                // find next match of stage 3 with 
                index = SuspiciousList.FirstOrDefault(a => a > tmpIndex);
                if (index > 0  && index != int.MaxValue)
                    return CalculateFault(stagelist, SuspiciousList, index, true, counter);

                return counter;
            }
            return counter;

        }
        #endregion

        #region GenerateList of Entity
        public List<StageEntity>  GenerateList(StreamReader eventLog) {
			string line = string.Empty;
			int stageNumber = -1;
			DateTime? stageTime = null;
		
			var objList = new List<StageEntity>();

			using (eventLog)
			{
				//Read eventLog and split its column based on tab
				while ((line = eventLog.ReadLine()) != null)
				{
					char[] delimiters = new char[] { '\t' };
					string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

					//if current stage is equal with prev stage is redundant information
					if (Convert.ToInt32(parts[1]) != stageNumber)
					{
						var objClass = new StageEntity()
						{
							
							StageNumber = Convert.ToInt32(parts[1]),
							StageTimeSpan = Convert.ToDateTime(parts[0])
						};
						//Calculate Time Different
						if (stageTime != null)
							objClass.StageDiffTime = objClass.StageTimeSpan.Subtract(Convert.ToDateTime(stageTime)).TotalMinutes;

						objList.Add(objClass);
						
					}
					stageNumber = Convert.ToInt32(parts[1]);
					stageTime = Convert.ToDateTime(parts[0]);

				}
			}
            return objList;
        }
        #endregion

    }
    /// <summary>
    /// this class is used to create an enity for each line of stream and read it and add to a list
    /// in order to count fault in next step
    /// </summary>
    public class StageEntity{
        
        /// <summary>
        /// Gets or sets the stage number.
        /// </summary>
        /// <value>The stage number.</value>
        public int StageNumber { get; set; }

        /// <summary>
        /// Gets or sets the stage time span.
        /// </summary>
        /// <value>The stage time span.</value>
        public DateTime StageTimeSpan { get; set; }

        /// <summary>
        /// hold differnstiate time between current stage and previous stage
        /// </summary>
        /// <value>The stage diff time.</value>
        public double StageDiffTime { get; set; }
    }


}
