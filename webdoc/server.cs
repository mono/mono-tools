//
// Monodoc server
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//

using System;
using System.Collections;
using System.IO;
using System.Web.Mail;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;
using ByteFX.Data.MySqlClient;
using System.Xml;

namespace Monodoc {
	[WebServiceAttribute (Description="Web service for the MonoDoc contribution system")]
	public class Contributions : System.Web.Services.WebService
	{
		const string basedir = "/home/contributions/";
		//const string basedir = "/tmp/contributions/";
		static string connection_string;
		
		static Contributions ()
		{
			using (StreamReader sr = new StreamReader (File.OpenRead ("connection.string"))){
				connection_string = sr.ReadLine ();
				Console.WriteLine ("Connection: " + connection_string);
			}
		}
		
                private IDbConnection GetConnection() 
                {
    			return new MySqlConnection(connection_string);
                }

                private MySqlParameter CreateParameter(string name, object value)
                {
                        return new MySqlParameter (name, value);
                }

		static void mail (string recipient, string body)
		{
			MailMessage m = new MailMessage ();
			m.From = "mono-docs-list@ximian.com";
			m.To = recipient;
			m.Subject = "Your Monodoc passkey";
			m.Body = String.Format ("\n\nWelcome to the Mono Documentation Effort,\n\n" + 
						"This is your passkey for contributing to the Mono Documentation effort:\n " +
						"       {0}\n\n" +
						"The Mono Documentation Team (mono-docs-list@ximian.com)", body);
			
			SmtpMail.SmtpServer = "localhost";
			SmtpMail.Send (m);
		}

		//
		// 0  => OK to send contributions.
		// -1 => Invalid version
		//
		[WebMethod(Description="Check the client/server version;  0 means that the server can consume your data")]
		public int CheckVersion (int version)
		{
			if (version == 1)
				return 0;
			return -1;
		}
		
		//
		// Return codes:
		//    -3 invalid characters in login
		//    -2 Login already registered, password resent.
		//    -1 Generic error
		//     0 password mailed
		//
		[WebMethod(Description="Requests a registration for a login")]
		public int Register (string login)
		{
			if (login.IndexOf ("'") != -1)
				return -3;
				
                        IDbConnection conn = GetConnection();
			conn.Open();
                        try 
                        {
                                IDbCommand cmd = conn.CreateCommand();
                                cmd.CommandText = "select password from person where name=@login";
                                cmd.Parameters.Add( CreateParameter("@login", login));
				IDataReader reader = cmd.ExecuteReader ();

				if (reader.Read ()){
					string password = (string) reader ["password"];
					mail (login, password);
					reader.Close ();
					return -2;
				}
				reader.Close ();
				Random rnd = new Random ();
				int pass = rnd.Next ();
				cmd.CommandText = "INSERT INTO person (name, password, last_serial) VALUES " +
                                                  "(@name, @password, 0)";
                                cmd.Parameters.Add( CreateParameter("@name",login));
                                cmd.Parameters.Add( CreateParameter("@password",pass));

                                cmd.ExecuteNonQuery ();
				mail (login, pass.ToString ());
				
				return 0;
			} catch (Exception e) {
				Console.Error.WriteLine (e);
			} finally {
				conn.Close ();
			}
			return -1;
		}
			
		[WebMethod (Description="Returns the latest serial number used for a change on the server")]
		public int GetSerial (string login, string password)
		{
                        IDbConnection conn = GetConnection();
			conn.Open();
                        try 
                        {
                                IDbCommand cmd = conn.CreateCommand();
                                cmd.CommandText = "select last_serial from person where name=@login and password=@password";
                                cmd.Parameters.Add( CreateParameter("@login", login));
                                cmd.Parameters.Add( CreateParameter("@password", password));
                                
                                object r = cmd.ExecuteScalar();
				if (r != null){
					Console.Error.WriteLine (r);
					return (int) r;
				}
                                return -1;
                        } catch (Exception e){
				Console.Error.WriteLine ("Exception" + e);
			} finally {
                                conn.Close();
                        }
                        return -1;
  		}

		// -1 Generic error.
		// -2 Erroneous XML
		int a=1;
		[WebMethod (Description="Submits a GlobalChangeSet as a contribution")]
		public int Submit (string login, string password, XmlNode node)
		{
			IDbConnection conn = GetConnection();
			conn.Open();
			try {
				IDbCommand cmd = conn.CreateCommand();
                                cmd.CommandText = "select * from person where name=@login and password=@password";
                                cmd.Parameters.Add( CreateParameter("@login", login));
                                cmd.Parameters.Add( CreateParameter("@password", password));

				IDataReader reader = cmd.ExecuteReader ();
				
				int ret_val = -1;
				
				if (reader.Read()){
					int id = (int)reader["person_id"]; 
					int serial = (int)reader["last_serial"]; 
					reader.Close ();
					
					//
					// Validate the XML
					//
					XmlDocument d = new XmlDocument ();
					d.AppendChild (d.ImportNode (node, true));
					XmlNodeReader r = new XmlNodeReader (d);
					try {
						object rr = GlobalChangeset.serializer.Deserialize (r);
					} catch {
						return -2;
					}
					
					string dudebase = basedir + id;
					Directory.CreateDirectory (dudebase);
					
					d.Save (dudebase + "/" + serial + ".xml");
					IDbTransaction txn = conn.BeginTransaction();
					try {
						cmd.CommandText = "UPDATE person SET last_serial=@last_serial WHERE name=@name AND password=@pwd";
						cmd.Parameters.Add( CreateParameter("@last_serial", serial+1));
						cmd.Parameters.Add( CreateParameter("@name", login));
						cmd.Parameters.Add( CreateParameter("@pwd", password));
						cmd.ExecuteNonQuery ();

						
                                                cmd.CommandText = "INSERT INTO status (person_id, serial, status) VALUES (@id, @serial, 0)";
                                                cmd.Parameters.Add( CreateParameter("@id",id));
                                                cmd.Parameters.Add( CreateParameter("@serial",serial));
						cmd.ExecuteNonQuery ();
						
						txn.Commit();
					} catch (Exception e) {
						Console.Error.WriteLine ("E: " + e);
					}
					
					ret_val = serial+1;
					return ret_val;
				}
				Console.Error.WriteLine ("Error, going: 4");
				return -4;
			} catch (Exception e) {
				Console.Error.WriteLine ("Failure in Submit: " + e);
				return -3;
			} finally {
				conn.Close ();
			}
		}

		bool IsAdmin (IDbConnection conn, string login, string password)
		{
			IDbCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "select person_id,is_admin from person where name=@name and password=@pass";
                        cmd.Parameters.Add( CreateParameter("@name",login));
                        cmd.Parameters.Add( CreateParameter("@pass",password));
                        
			int person_id = -1;
			bool is_admin = false;
			using (IDataReader reader = cmd.ExecuteReader ()){
				if (reader.Read ()){
					person_id = (int) reader ["person_id"];
					is_admin = ((int) reader ["is_admin"]) == 1;
				} else
					return false;
			}
			if (person_id == -1 || is_admin == false)
				return false;

			return true;
		}
		
		[WebMethod (Description="Obtains the list of pending contributions")]
		public PendingChange [] GetPendingChanges (string login, string password)
		{
			IDbConnection conn = GetConnection();
			conn.Open ();
			
			try {
				if (!IsAdmin (conn, login, password)){
					return new PendingChange [0];
				}
				
				IDbCommand cmd = conn.CreateCommand();
				ArrayList results = new ArrayList ();
				cmd.CommandText = "select status.person_id, serial, person.name from status, person where status=0 and person.person_id = status.person_id";
				using (IDataReader reader = cmd.ExecuteReader ()){
					while (reader.Read ()){
						results.Add (new PendingChange ((string) reader ["name"], (int) reader ["person_id"], (int) reader ["serial"]));
					}
				}

				PendingChange [] ret = new PendingChange [results.Count];
				results.CopyTo (ret);
				return ret;
			} catch (Exception e){
				Console.Error.WriteLine (e);
				return null;
			} finally {
				conn.Close ();
			}
		}

		[WebMethod (Description="Obtains a change set for a user")]
		public XmlNode FetchContribution (string login, string password, int person_id, int serial)
		{
			IDbConnection conn = GetConnection ();
			conn.Open ();
			try {
				if (!IsAdmin (conn, login, password))
					return null;

				XmlDocument d = new XmlDocument ();
				string fname = basedir + person_id + "/" + serial + ".xml";
				d.Load (fname);
				return d.FirstChild;
			} finally {
				conn.Close ();
			}
		}

		[WebMethod (Description="ADMIN: Obtains the number of pending commits")]
		public Status GetStatus (string login, string password)
		{
			IDbConnection conn = GetConnection ();
			conn.Open ();
			try {
				IDbCommand cmd = conn.CreateCommand();
                                cmd.CommandText = "select * from person where name=@name and password=@pass";
                                cmd.Parameters.Add( CreateParameter("@name",login));
                                cmd.Parameters.Add( CreateParameter("@pass",password));
                                
				IDataReader reader = cmd.ExecuteReader ();
				int id = -1;
				
				if (reader.Read())
					id = (int)reader["person_id"]; 
				reader.Close ();
				if (id == -1)
					return null;

				Status s = new Status ();
				
				cmd.CommandText = String.Format ("select count(*) from status where person_id='{0}'", id);
				s.Contributions =  (int) cmd.ExecuteScalar ();
				cmd.CommandText = String.Format ("select count(*) from status where person_id='{0}' and status='0'", id);
				s.Pending = (int) cmd.ExecuteScalar ();
				cmd.CommandText = String.Format ("select count(*) from status where person_id='{0}' and status='1'", id);
				s.Commited = (int) cmd.ExecuteScalar ();

				return s;
			} finally {
				conn.Close ();
			}
		}

		[WebMethod (Description="ADMIN: Updates the status of a contribution")]
		public void UpdateStatus (string login, string password, int person_id, int contrib_id, int status)
		{
			IDbConnection conn = GetConnection();
			conn.Open ();
			
			try {
				if (!IsAdmin (conn, login, password))
					return;
				
				IDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = "update status set status=@status WHERE person_id=@PID AND serial=@ser";
				cmd.Parameters.Add (CreateParameter ("@status", status));
				cmd.Parameters.Add (CreateParameter ("@PID", person_id));
				cmd.Parameters.Add (CreateParameter ("@ser", contrib_id));
				cmd.ExecuteNonQuery ();
			} finally {
				conn.Close ();
			}
		}
	}

	public class Status {
		public int Contributions;
		public int Commited;
		public int Pending;
	}
	
	public class PendingChange {
		public string Login;
		public int ID;
		public int Serial;
		
		public PendingChange (string login, int person_id, int serial)
		{
			Login = login;
			ID = person_id;
			Serial = serial;
		}

		public PendingChange ()
		{
		}
	}
}
