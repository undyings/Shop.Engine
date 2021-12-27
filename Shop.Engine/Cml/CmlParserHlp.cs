using Commune.Basis;
using Commune.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shop.Engine.Cml
{
	public static class CmlParserHlp
	{
		class ImageInfo
		{
			public int ObjectId;
			public string ImagePath;

			public ImageInfo(int objectId, string imagePath)
			{
				this.ObjectId = objectId;
				this.ImagePath = imagePath;
			}
		}

		public static bool IsEndElement(this XmlTextReader reader, string elementName)
		{
			reader.IsStartElement();

			bool result = reader.Name == elementName && 
				(reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement);

			//if (result)
			//	Logger.AddMessage("IsEnd: {0}, {1}, {2}", reader.NodeType, reader.Name, reader.IsEmptyElement);

			return result;
		}

		public static void ParseOffersXml(IDataLayer fabricConnection, Stream stream)
		{
			ObjectBox fabricBox = new ObjectBox(fabricConnection, DataCondition.ForTypes(FabricType.Fabric));

			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				while (reader.Read())
				{
					if (reader.IsStartElement("ПакетПредложений"))
					{
						while (reader.Read() && !reader.IsEndElement("ПакетПредложений"))
						{
							if (reader.IsStartElement("Предложения"))
							{
								while (reader.Read() && !reader.IsEndElement("Предложения"))
								{
									if (reader.IsStartElement("Предложение"))
									{
										string id = null;
										int? price = null;
										int? amount = null;

										while (reader.Read() && !reader.IsEndElement("Предложение"))
										{
											if (reader.IsStartElement("Ид"))
												id = reader.ReadString();
											else if (reader.IsStartElement("Цены"))
											{
												while (reader.Read() && !reader.IsEndElement("Цены"))
												{
													if (reader.IsStartElement("Цена"))
													{
														while (reader.Read() && !reader.IsEndElement("Цена"))
														{
															if (reader.IsStartElement("ЦенаЗаЕдиницу"))
															{
																price = ConvertHlp.ToInt(reader.ReadString());
															}
														}
													}
												}
											}
											else if (reader.IsStartElement("Количество"))
												amount = ConvertHlp.ToInt(reader.ReadString());
										}

										//Logger.AddMessage("Offer: {0}, {1}, {2}", id, price, amount);

										CmlSyncHlp.SyncFabricOffer(fabricBox, id, price, amount);
									}
								}
							}
						}
					}
				}
			}

			fabricBox.Update();
		}

		public static void ParseFabricsXml(IDataLayer fabricConnection, Stream stream, 
			string rootPath, bool groupFromCategory)
		{
			ParentBox groupBox = new ParentBox(fabricConnection, DataCondition.ForTypes(GroupType.Group));
			ObjectBox propertyBox = new ObjectBox(fabricConnection, DataCondition.ForTypes(MetaPropertyType.Property));
			ParentBox kindBox = new ParentBox(fabricConnection,	DataCondition.ForTypes(MetaKindType.FabricKind));
			ObjectBox fabricBox = new ObjectBox(fabricConnection, DataCondition.ForTypes(FabricType.Fabric));

			List<LightObject> fabrics = new List<LightObject>();
			List<ImageInfo> imageInfos = new List<ImageInfo>();

			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				while (reader.Read())
				{
					//Logger.AddMessage("Level1: {0}, {1}", reader.NodeType, reader.Name);

					if (reader.IsStartElement("Классификатор"))
					{
						while (reader.Read() && !reader.IsEndElement("Классификатор"))
						{
							//Logger.AddMessage("Level1: {0}, {1}", reader.NodeType, reader.Name);

							if (reader.IsStartElement("Группы"))
							{
								while (reader.Read() && !reader.IsEndElement("Группы"))
								{
									if (reader.IsStartElement("Группа"))
									{
										//Logger.AddMessage("Группа: {0}, {1}", reader.NodeType, reader.Name);

										string id = null;
										string name = null;
										while (reader.Read() && !reader.IsEndElement("Группа"))
										{
											//reader.IsStartElement();

											//Logger.AddMessage("Группа.Inner: {0}, {1}", reader.NodeType, reader.Name);

											if (reader.IsStartElement("Ид"))
												id = reader.ReadString();
											else if (reader.IsStartElement("Наименование"))
												name = reader.ReadString();
										}
										//Logger.AddMessage("Группа: {0}, {1}", id, name);

										if (!groupFromCategory)
											CmlSyncHlp.SyncOrCreateGroup(groupBox, id, name);
									}
								}
							}
							else if (reader.IsStartElement("Свойства"))
							{
								while (reader.Read() && !reader.IsEndElement("Свойства"))
								{
									if (reader.IsStartElement("Свойство"))
									{
										string id = null;
										string name = null;
										string kind = null;
										List<PropertyItem> enumItems = new List<PropertyItem>();
										while (reader.Read() && !reader.IsEndElement("Свойство"))
										{
											if (reader.IsStartElement("Ид"))
												id = reader.ReadString();
											else if (reader.IsStartElement("Наименование"))
												name = reader.ReadString();
											else if (reader.IsStartElement("ТипЗначений"))
												kind = reader.ReadString();
											else if (reader.IsStartElement("ВариантыЗначений") && kind == "Справочник")
											{
												while (reader.Read() && !reader.IsEndElement("ВариантыЗначений"))
												{
													if (reader.IsStartElement("Справочник"))
													{
														string itemId = null;
														string itemValue = null;
														while (reader.Read() && !reader.IsEndElement("Справочник"))
														{
															if (reader.IsStartElement("ИдЗначения"))
																itemId = reader.ReadString();
															else if (reader.IsStartElement("Значение"))
																itemValue = reader.ReadString();
														}

														if (itemId != null && itemValue != null)
															enumItems.Add(new PropertyItem(itemId, itemValue));
													}
												}
											}
										}

										CmlSyncHlp.SyncOrCreateProperty(propertyBox, id, name, kind, enumItems.ToArray());
									}
								}
							}
							else if (reader.IsStartElement("Категории"))
							{
								while (reader.Read() && !reader.IsEndElement("Категории"))
								{
									if (reader.IsStartElement("Категория"))
									{
										string id = null;
										string name = null;
										List<string> propertyGuids = new List<string>();

										while (reader.Read() && !reader.IsEndElement("Категория"))
										{
											if (reader.IsStartElement("Ид"))
												id = reader.ReadString();
											else if (reader.IsStartElement("Наименование"))
												name = reader.ReadString();
											else if (reader.IsStartElement("Свойства"))
											{
												while (reader.Read() && !reader.IsEndElement("Свойства"))
												{
													if (reader.IsStartElement("Ид"))
														propertyGuids.Add(reader.ReadString());
												}
											}
										}

										CmlSyncHlp.SyncOrCreateFabricKind(kindBox, propertyBox, id, name, propertyGuids);

										if (groupFromCategory)
											CmlSyncHlp.SyncOrCreateGroup(groupBox, id, name);
									}
								}
							}
						}
					}
					else if (reader.IsStartElement("Каталог"))
					{
						while (reader.Read() && !reader.IsEndElement("Каталог"))
						{
							if (reader.IsStartElement("Товары"))
							{
								Dictionary<string, ValueItem> valuesIndices = CmlHlp.CreateValueIndices(propertyBox);

								while (reader.Read() && !reader.IsEndElement("Товары"))
								{
									if (reader.IsStartElement("Товар"))
									{
										string id = null;
										string name = null;
										List<string> groupIds = new List<string>();
										string categoryId = null;
										string description = null;
										string imagePath = null;
										List<Option> properties = new List<Option>();

										while (reader.Read() && !reader.IsEndElement("Товар"))
										{
											if (reader.IsStartElement("Ид"))
												id = reader.ReadString();
											else if (reader.IsStartElement("Наименование"))
												name = reader.ReadString();
											else if (reader.IsStartElement("Группы"))
											{
												while (reader.Read() && !reader.IsEndElement("Группы"))
												{
													if (reader.IsStartElement("Ид"))
														groupIds.Add(reader.ReadString());
												}
											}
											else if (reader.IsStartElement("Категория"))
												categoryId = reader.ReadString();
											else if (reader.IsStartElement("Описание"))
												description = reader.ReadString();
											else if (reader.IsStartElement("Картинка"))
												imagePath = reader.ReadString();
											else if (reader.IsStartElement("ЗначенияСвойств"))
											{
												while (reader.Read() && !reader.IsEndElement("ЗначенияСвойств"))
												{
													if (reader.IsStartElement("ЗначенияСвойства"))
													{
														string propertyId = null;
														string valueId = null;
														while (reader.Read() && !reader.IsEndElement("ЗначенияСвойства"))
														{
															if (reader.IsStartElement("Ид"))
																propertyId = reader.ReadString();
															else if (reader.IsStartElement("Значение"))
																valueId = reader.ReadString();
														}

														properties.Add(new Option(propertyId, valueId));
													}
												}
											}
											else if (reader.IsStartElement("ЗначенияРеквизитов"))
											{
												while (reader.Read() && !reader.IsEndElement("ЗначенияРеквизитов"))
												{
												}
											}
										}

										//Logger.AddMessage("Товар: {0}, {1}, {2}, {3}, {4}, {5}",
										//	id, name, groupIds.FirstOrDefault(), categoryId, imagePath, properties.Count
										//);

										LightObject fabric = CmlSyncHlp.SyncOrCreateFabric(groupBox, kindBox, valuesIndices,
											fabricBox, id, name, groupIds, categoryId, description, properties
										);

										if (fabric != null)
										{
											fabrics.Add(fabric);
											imageInfos.Add(new ImageInfo(fabric.Id, imagePath));
										}
									}
								}
							}
						}
					}

				}
			}

			groupBox.Update();
			propertyBox.Update();
			kindBox.Update();
			fabricBox.Update();

			foreach (ImageInfo info in imageInfos)
			{
				try
				{
					CmlSyncHlp.SyncObjectImage(info.ObjectId, rootPath, info.ImagePath);
				}
				catch (Exception ex)
				{
					Logger.WriteException(ex, "Ошибка при синхронизации картинки объекта: {0}, {1}",
						info.ObjectId, info.ImagePath
					);
				}
			}

			CollectionSynchronizer.Synchronize(
				fabricBox.AllObjectIds, delegate (int fabricId) { return fabricId; },
				fabrics, delegate (LightObject fabric) { return fabric.Id; },
				delegate (int removeFabricId)
				{
					LightObject removeFabric = new LightObject(fabricBox, removeFabricId);
					Logger.AddMessage("Удаляем товар {0}, {1}, {2}", 
						removeFabricId, removeFabric.Get(FabricType.Identifier), FabricType.DisplayName(removeFabric)
					);
					SQLiteDatabaseHlp.DeleteParentObject(fabricConnection, removeFabricId);
				},
				delegate (LightObject addFabric)
				{
					Logger.AddMessage("Добавлен товар {0}, {1}, {2}",
						addFabric.Id, addFabric.Get(FabricType.Identifier), FabricType.DisplayName(addFabric)
					);
				},
				delegate (int fabricId, LightObject fabric)
				{
				}
			);

			Logger.AddMessage("ParseXml.Finish");
		}
	}
}
