import sqlite3

PATH = 'instrument.db'


def create_db(c):
    

    ##########################################################################
    ##################  T A B L E       C R E A T I O N S   ##################
    ##################                                      ##################
    ##########################################################################

    '''
    [SESSIONS] table
    '''
    c.execute('''CREATE TABLE IF NOT EXISTS sessions(ID INTEGER PRIMARY KEY AUTOINCREMENT,is_over INTEGER,name VARCHAR(160),total_coverage VARCHAR(10),start_time DATETIME,end_time DATETIME,current_active_modules_count INTEGER,total_executable INTEGER,total_executed INTEGER)''')

    '''
    [STATS] table
    '''
    c.execute('''CREATE TABLE IF NOT EXISTS stats(ID INTEGER PRIMARY KEY AUTOINCREMENT,file_id INTEGER, session_id INTEGER, date DATETIME, filename VARCHAR(4000), line INTEGER, line_guid VARCHAR(1000), coverage_type VARCHAR(200),send_time DATETIME,custom_value VARCHAR(200))''')

    '''
   [CONFIG] table
    '''
    c.execute('''CREATE TABLE IF NOT EXISTS config(ID INTEGER PRIMARY KEY AUTOINCREMENT,name VARCHAR(100), value VARCHAR(200))''')

    '''
    [FILES] table
    When upload package contains file that does not exist in db, insert file along with content. If upload package contains file
    that exists in db with history=true, make it history=false. If db contains file that is not in upload package mark that file as history.

    ID
    name
    path
    type
    should_instrument
    is_history - if there is an upload of files from instrumenter and some file exists in db but not in upload package, mark the file as history=true.
    '''
    c.execute('''CREATE TABLE IF NOT EXISTS files(ID INTEGER PRIMARY KEY AUTOINCREMENT,name VARCHAR(100), path VARCHAR(4000), type VARCHAR(20), should_instrument INTEGER, is_history INTEGER)''')

    '''
    [VISITED_ROUTES] table

    '''
    c.execute('''CREATE TABLE IF NOT EXISTS visited_routes(ID INTEGER PRIMARY KEY AUTOINCREMENT,route_visited VARCHAR(100),session_id INTEGER)''')

    '''
    [FILE_DETAILS] table
    ID
    file_id
    file_content
    executable_lines_count

    '''
    c.execute(
        '''CREATE TABLE IF NOT EXISTS file_details(ID INTEGER PRIMARY KEY AUTOINCREMENT,file_id INTEGER, file_content BLOB, executable_lines_count INTEGER,updated DATETIME,executable_lines VARCHAR(4000))''')

    '''
    [ROUTES] table
    ID
    route
    is_history - if upload package hasn't got route that exist in db, mark that as history=1 for reference
    '''
    c.execute(
        '''CREATE TABLE IF NOT EXISTS routes(ID INTEGER PRIMARY KEY AUTOINCREMENT,route VARCHAR(100),is_history INTEGER)''')

    '''
    [MODULES] table

    ID
    module_name - unique module name given by user in Modules section of the dashboard
    related_files - comma separated file id's
    last_update - date when last update was performed which resulted in revision being increased
    is_removed - true if user completely removed it (0/1); can set history=1 as well.
    operation - files_update,rename,delete
    is_history -when user updates module, eg: renames or file assignment then current module's entry becomes history; new entry is inserted for updated state.
    '''
    c.execute(
        '''CREATE TABLE IF NOT EXISTS modules(ID INTEGER PRIMARY KEY AUTOINCREMENT,module_name VARCHAR(100),related_files VARCHAR(4000),last_update DATETIME,is_removed INTEGER,operation VARCHAR(100),is_history INTEGER)''')

    '''
    [COVERED_MODULES] table
    when session starts, list of modules is specified - can select all or 1 at minimum
    this table reflects that


    ID
    module_id - module id that is related to session; this is ID of module entry from modules table.
    session_id
    total_coverage - total coverage for the whole module (all files in it combined) expressed in percentage

    '''
    c.execute('''CREATE TABLE IF NOT EXISTS covered_modules(ID INTEGER PRIMARY KEY AUTOINCREMENT,module_id INTEGER,session_id INTEGER,total_coverage VARCHAR(10))''')

    '''
    [SESSIONS_FILES] table
    files included in session are the ones belonging to modules chosen when creating session
    example: chosen 1 module out of 5, so only files from module 1 are in session.


    '''
    c.execute(
        '''CREATE TABLE IF NOT EXISTS sessions_files(session_id INTEGER, file_id INTEGER,file_details INTEGER)''')

    '''
    [USERS] table
    each test session must have some owner. Owner means user. This table
    holds users, most likely testers of the system for which coverage is being calculated.
    default: generic_tester
    '''
    create_users_table(c)

    '''
    [TAGS] table
    '''
    create_tags_table(c)

    '''
    [SESSIONS_USERS_TAGS] table
    table holding relation between session and users and tags
    '''
    create_sessions_users_tags_table(c)

    '''
    [BUILDS] table
    '''
    create_builds_table(c)

    '''
    [SESSIONS_BUILDS]
    '''
    create_session_build_table(c)


    ##########################################################################
    ##################  C O N F I G     E N T R I E S   ######################
    ##################  AND DEFAULTS                    ######################
    ##########################################################################

    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("SESSION_TIMEOUT", 3600))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)", ("PORT", 5000))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("UPDATE_INTERVAL_SECONDS", 10))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("SERVER_HOST", "0.0.0.0"))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("VERSION", "3.0.0"))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("PROJECT_NAME", "Sample Project"))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("DEFAULT_MIN_COVERAGE_PERCENT", 60))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("BUFFER_TIME_BEFORE_CLOSING_SESSION_SECONDS", 10))

   
    

    


def create_tags_table(cursor):
    cursor.execute(
        '''CREATE TABLE IF NOT EXISTS tags(ID INTEGER PRIMARY KEY AUTOINCREMENT,tag VARCHAR(100),is_history INTEGER)''')
    # default tag
    cursor.execute("INSERT INTO tags(ID,tag,is_history) VALUES(?,?,?)",
              (0,"general",0))

def create_users_table(cursor):
     cursor.execute(
        '''CREATE TABLE IF NOT EXISTS users(ID INTEGER PRIMARY KEY AUTOINCREMENT,username VARCHAR(100),is_history INTEGER)''')
      # default user
     cursor.execute("INSERT INTO users(ID,username,is_history) VALUES(?,?,?)",
              (0,"generic_tester",0))

def create_sessions_users_tags_table(cursor):
    cursor.execute(
        '''CREATE TABLE IF NOT EXISTS sessions_users_tags(session_id INTEGER,user_id INTEGER,tag_id INTEGER)''')
    
def create_builds_table(cursor):
    cursor.execute(
        '''CREATE TABLE IF NOT EXISTS builds(ID INTEGER PRIMARY KEY AUTOINCREMENT,build INTEGER,tag_id INTEGER,update_date DATETIME)''')
    cursor.execute("INSERT INTO builds(ID,build,tag_id,update_date) VALUES(?,?,?,?)",
              (0,0,0,None))

def create_session_build_table(cursor):
    cursor.execute(
        '''CREATE TABLE IF NOT EXISTS sessions_builds(session_id INTEGER,build_id INTEGER)''')


def update_stats_table_to_v3(cursor):
    cursor.execute(
        '''ALTER TABLE stats ADD COLUMN send_time DATETIME''')
    cursor.execute(
        '''ALTER TABLE stats ADD COLUMN custom_value VARCHAR(200)''')


def update_sessions_table_to_v3(cursor):
    cursor.execute(
        '''ALTER TABLE sessions ADD COLUMN total_executable INTEGER''')
    cursor.execute(
        '''ALTER TABLE sessions ADD COLUMN total_executed INTEGER''')


def create_connection():
    conn = sqlite3.connect(PATH)
    c = conn.cursor()
    return conn,c

def close_connection(connection):
    connection.commit()
    connection.close()

if __name__ == '__main__':
    connection,cursor=create_connection()
    create_db(cursor)
    close_connection(connection)
