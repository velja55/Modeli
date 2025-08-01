﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Wires;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.LoadModel;


namespace FTN.Services.NetworkModelService
{		
	public class Container
	{
		/// <summary>
		/// The dictionary of entities. Key = GlobaId, Value = Entity
		/// </summary>		
		private Dictionary<long, IdentifiedObject> entities = new Dictionary<long, IdentifiedObject>();	

		/// <summary>
		/// Initializes a new instance of the Container class
		/// </summary>
		public Container()
		{
		}

		/// <summary>
		/// Gets or sets dictionary of entities (identified objects) inside container.
		/// </summary>	
		public Dictionary<long, IdentifiedObject> Entities
		{
			get { return entities; }
			set { entities = value; }
		}

		/// <summary>
		/// Gets a number of entitis in container
		/// </summary>		
		public int Count
		{
			get { return entities.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether container is empty
		/// </summary>		
		public bool IsEmpty
		{
			get { return entities.Count == 0; }			
		}
			
		# region operators

		public static bool operator ==(Container x, Container y)
		{
			if (Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null))
			{
				return true;
			}
			else if ((Object.ReferenceEquals(x, null) && !Object.ReferenceEquals(y, null)) || (!Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null)))
			{
				return false;
			}
			else
			{
				// TO DO
				if (x.entities.Count != y.entities.Count)
				{
					return false;
				}

				IdentifiedObject io = null;

				foreach (KeyValuePair<long, IdentifiedObject> pair in x.Entities)
				{
					//if (!y.objects.ContainsKey(pair.Key))
					if (y.Entities.TryGetValue(pair.Key, out io))
					{
						return false;
					}
					else if (io != pair.Value)
					{
						return false;
					}
				}

				 return true;									
			}
		}

		public static bool operator !=(Container x, Container y)
		{
			return !(x == y);
		}

        public override bool Equals (object obj)
        {
			return this == (Container)obj;
        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}				

		# endregion operators

		/// <summary>
		/// Creates entity for specified global inside the container.
		/// </summary>
		/// <param name="globalId">Global id of the entity for insert</param>		
		/// <returns>Created entity (identified object).</returns>
		public IdentifiedObject CreateEntity(long globalId)
		{
			short type = ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

			IdentifiedObject io = null;			
			switch ((DMSType)type)
			{
				case DMSType.TERMINAL:
					io = new Terminal(globalId);
					break;
				case DMSType.REGULATIONSCHEDULE:
					io = new RegulationSchedule(globalId);
					break;
				case DMSType.REGULATINGCONTROL:
					io = new RegulatingControl(globalId);
					break;
				case DMSType.STATICVARCOMPENSATOR:
					io = new StaticVarCompensator(globalId);
					break;
				case DMSType.SHUNTCOMPENSATOR:
					io = new ShuntCompensator(globalId);
					break;
                case DMSType.DAYTYPE:
                    io = new DayType(globalId);
                    break;
                default:					
					string message = String.Format("Failed to create entity because specified type ({0}) is not supported.", type);
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);					
			}

            // Add entity to map
			this.AddEntity(io);

			return io;
		}		

		/// <summary>
		/// Checks if entity exists in container.
		/// </summary>
		/// <param name="globalId">Global id of the entity that should be checked</param>
		/// <returns>TRUE if the entity is found.</returns>
		public bool EntityExists(long globalId)
		{
			return entities.ContainsKey(globalId);
		}	

		/// <summary>
		/// Returns entity (identified object) on the specified index. Throws an exception if entity does not exist. 
		/// </summary>
		/// <param name="index">Index of the entity that should be returned</param>
		/// <returns>Instance of the entity in case it is found on specified position, otherwise throws exception</returns>
		public IdentifiedObject GetEntity(long globalId)
		{
			if (EntityExists(globalId))
			{
				return entities[globalId];
			}
			else
			{
				string message = String.Format("Failed to retrieve entity (GID = 0x{1:x16}) because entity doesn't exist.", globalId);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Adds entity on the first free position in the container.
		/// </summary>
		/// <param name="io">Entity (identified object) that should be added</param>
		/// <returns>Index of the entity that is just added.</returns>
		public void AddEntity(IdentifiedObject io)
		{
			if (!EntityExists(io.GlobalId))
			{
				entities[io.GlobalId] = io;
			}
			else
			{
				string message = String.Format("Entity (GID = 0x{1:x16}) already exists.", io.GlobalId);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Removes entity from the container at the specified position. Throws an exception if entity does not exist.
		/// </summary>
		/// <param name="index">Index of the entity that should be removed</param>
		/// <returns>Returns entity that is removed</returns>
		public IdentifiedObject RemoveEntity(long globalId)
		{
			IdentifiedObject io = null;
			if (EntityExists(globalId))
			{
				entities.Remove(globalId);
			}
			else
			{
				string message = String.Format("Failed to remove entity because entity with GID = {0} doesn't exist at the specified position ( {0} ).", globalId);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}

			return io;
		}

		/// <summary>
		/// Get globalIds of all entities.
		/// </summary>		
		/// <returns>Returns globalIds of all entities</returns>
		public List<long> GetEntitiesGlobalIds()
		{
			return entities.Keys.ToList();
		}			
	}
}
