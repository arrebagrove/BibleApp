﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MXit.Messaging;
using MXit.Messaging.MessageElements;
using MXit.Messaging.MessageElements.Actions;
using MXit.Messaging.MessageElements.Replies;
using MXit.User;
using MXit;
using MXit.Log;


namespace MxitTestApp
{
    class VerseMessageSendHandler : AInputHandler
    {

        public override void init(UserSession us)
        {
            if (us.hasVariable(VerseMessageSendOutputAdapter.MESSAGE_SUBJECT))
            {
                us.deleteVariable(VerseMessageSendOutputAdapter.MESSAGE_SUBJECT);
            }
            if (us.hasVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID))
            {
                us.deleteVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID);
            } 
            if (us.hasVariable(VerseMessageSendOutputAdapter.MESSAGE_TEXT))
            {
                us.deleteVariable(VerseMessageSendOutputAdapter.MESSAGE_TEXT);
            }
            if (us.hasVariable(ChooseFriendHandler.RECIPIENT_LIST))
            {
                us.deleteVariable(ChooseFriendHandler.RECIPIENT_LIST);
            }
            if (us.hasVariable(ChooseFriendHandler.RECIPIENT_LIST))
                us.removeVariable(ChooseFriendHandler.RECIPIENT_LIST);
        }

        public override InputHandlerResult handleInput(UserSession user_session, MessageReceived message_recieved)
        {
            string input = extractReply(message_recieved);
            //Console.WriteLine("in input handler: " + input);
            Console.WriteLine("User with ID: " + user_session.user_profile.id + " Entered: " + input);            
            //get reply
            string curr_user_page = user_session.current_menu_loc;

            InputHandlerResult output = handleDisplayMessageLinks(
               user_session,
               input,
               "Your input was invalid. You message has been sent already but please click Back/Main to continue",
               true);

            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
            {
                if(output.action == InputHandlerResult.BACK_WITHOUT_INIT_MENU_ACTION)
                    user_session.setVariable(Browse_Bible_Handler.BROWSE_CLEAR_SCREEN, true);
                return output;
            }
            output = handleStdNavLinks(user_session, input,true);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            output = handleMyProfileLinks(user_session, input);
            if (output.action != (InputHandlerResult.UNDEFINED_MENU_ACTION))
                return output;

            //handle back or home here. 


            return new InputHandlerResult(
                    "Invalid entry...Please enter a valid input"); //invalid choice

        }

        /*this method either returns the new screen id or the main or prev command string*/
        protected InputHandlerResult handleMyProfileLinks(
            UserSession user_session,
            string input)
        {
            string curr_user_page = user_session.current_menu_loc;
            String entry = input.ToUpper();
            if (CHOOSE_FRIEND_ID.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.SEND_VERSE_CHOOSE_FRIEND_ID); //the menu id is retreived from the session in this case. 
            }
            else if (ENTER_MESSAGE_SUBJECT.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.SEND_VERSE_ENTER_SUBJECT); //the menu id is retreived from the session in this case. 
            }
            else if (ENTER_MESSAGE.Equals(entry))
            {
                return new InputHandlerResult(
                    InputHandlerResult.NEW_MENU_ACTION,
                    MenuIDConstants.SEND_VERSE_ENTER_MESSAGE); //the menu id is retreived from the session in this case. 
            }
            else if (SEND_MESSAGE.Equals(entry))
            {
                String message_text = "";
                //String recip_id_s = "";
                long recip_id = -1;
                String start_verse = "";
                String end_verse = "";
                String subject = "";

                if (user_session.hasVariable(VerseMessageSendOutputAdapter.MESSAGE_TEXT))
                    message_text = user_session.getVariable(VerseMessageSendOutputAdapter.MESSAGE_TEXT);

                if (user_session.hasVariable(VerseMessageSendOutputAdapter.MESSAGE_SUBJECT))
                    subject = user_session.getVariable(VerseMessageSendOutputAdapter.MESSAGE_SUBJECT);

                /*if (user_session.hasVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID))
                    recip_id= long.Parse(user_session.getVariable(VerseMessageSendOutputAdapter.FRIEND_TO_SEND_ID));
                else
                    return new InputHandlerResult("There is a problem in sending the message. Please let us know about this problem by using the feedback option");
                */ //old style sing recipient. 

                //new multip recipient. 
                List<long> recipient_list;
                if (user_session.hasVariable(ChooseFriendHandler.RECIPIENT_LIST))
                    recipient_list = (List<long>)user_session.getVariableObject(ChooseFriendHandler.RECIPIENT_LIST);
                else
                    return new InputHandlerResult("There is a problem in sending the message. Please let us know about this problem by using the feedback option");

                VerseSection vs = (VerseSection)user_session.getVariableObject("Browse.verse_section");
                if (vs == null)
                {
                    Console.WriteLine("Expected Browse.verse_section present, but not found in VerseMessageSendHandler.");
                    return new InputHandlerResult("There is a problem in sending the message. Please let us know about this problem by using the feedback option");
                }
                else
                {
                    Verse start = vs.start_verse;
                    Verse end = vs.end_verse;
                    if (end == null)
                    {
                        end = BrowseBibleScreenOutputAdapter.getDefaultEndVerse(start);
                    }
                    start_verse = start.getVerseReference();
                    end_verse = end.getVerseReference();
                }
                
                user_session.verse_messaging_manager.createThreadAndAddPrivateMessage(
                    message_text, 
                    recipient_list,
                    start_verse, 
                    end_verse, 
                    subject);
                return new InputHandlerResult(
                 InputHandlerResult.DISPLAY_MESSAGE,
                 InputHandlerResult.DEFAULT_MENU_ID, //not used
                 "Your Message has been sent...");
            }
            else
            {
                return new InputHandlerResult(
                    InputHandlerResult.UNDEFINED_MENU_ACTION,
                    InputHandlerResult.DEFAULT_MENU_ID,
                    InputHandlerResult.DEFAULT_PAGE_ID);
            }
        }

        public const String CHOOSE_FRIEND_ID = "SEND_TO_FRIEND";
        public const String ENTER_MESSAGE_SUBJECT = "ENTER_MESSAGE_SUBJECT";
        public const String ENTER_MESSAGE = "ENTER_MESSAGE";
        public const String SEND_MESSAGE = "SEND_MESSAGE";

        public const String REFRESH_PROFILE = "REFRESH_PROFILE";
       
    }


}
